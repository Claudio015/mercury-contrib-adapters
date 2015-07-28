#region Copyright
/*
Copyright 2014 Cluster Reply s.r.l.

Licensed under the Apache License, Version 2.0 (the "License");
you may not use this file except in compliance with the License.
You may obtain a copy of the License at

    http://www.apache.org/licenses/LICENSE-2.0

Unless required by applicable law or agreed to in writing, software
distributed under the License is distributed on an "AS IS" BASIS,
WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
See the License for the specific language governing permissions and
limitations under the License.
*/
#endregion

/// -----------------------------------------------------------------------------------------------------------
/// Module      :  FtpAdapterInboundHandler.cs
/// Description :  This class implements an interface for listening or polling for data.
/// -----------------------------------------------------------------------------------------------------------
/// 
#region Using Directives
using System;
using System.Collections.Generic;
using System.Text;

using Microsoft.ServiceModel.Channels.Common;
using System.Threading;
using System.Collections.Concurrent;
using System.IO;
using System.Net.FtpClient;
using System.Net;
using Reply.Cluster.Mercury.Adapters.Helpers;
using System.ServiceModel.Channels;
#endregion

namespace Reply.Cluster.Mercury.Adapters.Ftp
{
    public class FtpAdapterInboundHandler : FtpAdapterHandlerBase, IInboundHandler
    {
        private class FileItem
        {
            public FileItem(FtpClient client, string path, Stream stream)
            {
                Client = client;
                Path = path;
                Stream = stream;
            }

            public FtpClient Client { get; private set; }
            public string Path { get; private set; }
            public Stream Stream { get; private set; }
        }

        /// <summary>
        /// Initializes a new instance of the FtpAdapterInboundHandler class
        /// </summary>
        public FtpAdapterInboundHandler(FtpAdapterConnection connection
            , MetadataLookup metadataLookup)
            : base(connection, metadataLookup)
        {
            connectionUri = connection.ConnectionFactory.ConnectionUri;
            filter = new Wildcard(connectionUri.FileName);

            pollingType = connection.ConnectionFactory.Adapter.PollingType;

            if (pollingType == PollingType.Simple)
            {
                pollingInterval = connection.ConnectionFactory.Adapter.PollingInterval;
                pollingTimer = new Timer(new TimerCallback(t => GetFiles()));
            }
            else
                scheduleName = connection.ConnectionFactory.Adapter.ScheduleName;

            subFolders = new List<string>();
            if (string.IsNullOrEmpty(connection.ConnectionFactory.Adapter.SubFolders))
            {
                subFolders.Add(connectionUri.Path);
            }
            else
            {
                foreach (var item in connection.ConnectionFactory.Adapter.SubFolders.Split(';'))
                {
                    subFolders.Add(string.Concat(connectionUri.Path, "/", item));
                }
            }
            
        }

        #region Private Fields

        private FtpAdapterConnectionUri connectionUri;
        private Wildcard filter;

        private PollingType pollingType;

        private int pollingInterval;
        private Timer pollingTimer;
        private string scheduleName;

        private BlockingCollection<FileItem> queue = new BlockingCollection<FileItem>();
        private CancellationTokenSource cancelSource = new CancellationTokenSource();

        private List<string> subFolders;

        #endregion Private Fields

        #region IInboundHandler Members

        /// <summary>
        /// Start the listener
        /// </summary>
        public void StartListener(string[] actions, TimeSpan timeout)
        {
            if (pollingType == PollingType.Simple)
                pollingTimer.Change(0, pollingInterval * 1000);
            else
                ScheduleHelper.RegisterEvent(scheduleName, () => GetFiles());
        }

        /// <summary>
        /// Stop the listener
        /// </summary>
        public void StopListener(TimeSpan timeout)
        {
            if (pollingType == PollingType.Simple)
                pollingTimer.Change(Timeout.Infinite, Timeout.Infinite);
            else
                ScheduleHelper.CancelEvent(scheduleName);
                        
            queue.CompleteAdding();
            cancelSource.Cancel();
        }

        /// <summary>
        /// Tries to receive a message within a specified interval of time. 
        /// </summary>
        public bool TryReceive(TimeSpan timeout, out System.ServiceModel.Channels.Message message, out IInboundReply reply)
        {
            reply = null;
            message = null;

            if (queue.IsCompleted)
                return false;

            FileItem item = null;
            bool result = queue.TryTake(out item, (int)Math.Min(timeout.TotalMilliseconds, (long)int.MaxValue), cancelSource.Token);

            if (result)
            {
                Uri originalUri = connectionUri.Uri;

                message = ByteStreamMessage.CreateMessage(item.Stream);
                message.Headers.Action = new UriBuilder(originalUri.Scheme, originalUri.Host, originalUri.Port, item.Path).Uri.ToString();

                reply = new FtpAdapterInboundReply(item.Client, item.Path, item.Stream);
            }

            return result;
        }

        /// <summary>
        /// Returns a value that indicates whether a message has arrived within a specified interval of time.
        /// </summary>
        public bool WaitForMessage(TimeSpan timeout)
        {
            return true;
        }

        #endregion IInboundHandler Members

        #region Private Members

        private void GetFiles()
        {
            var client = Connection.Client;

            List<Tuple<DateTime, FileItem>> totalFiles = new List<Tuple<DateTime, FileItem>>();

            foreach (var folder in subFolders)
            {
                var files = client.GetListing(folder);

                foreach (var file in files)
                {
                    if (filter.IsMatch(file.Name))
                    {
                        string path = file.FullName;

                        try
                        {
                            var stream = client.OpenRead(path);
                            DateTime modDate = client.GetModifiedTime(path);
                            totalFiles.Add(new Tuple<DateTime, FileItem>(modDate, new FileItem(client, path, stream)));
                        }
                        catch (FtpException) { }
                    }
                }
            }
            totalFiles.Sort(new Comparison<Tuple<DateTime, FileItem>>((t1, t2) => 
            { 
                long diff = t1.Item1.Ticks - t2.Item1.Ticks;
                if (diff < 0)
                {
                    return -1;
                }
                if (diff > 0)
                {
                    return 1;
                }
                return 0;
            }));
            foreach (var item in totalFiles)
            {
                queue.Add(item.Item2);
            }

            //var files = client.GetListing(connectionUri.Path);

            //foreach (var file in files)
            //{
            //    if (filter.IsMatch(file.Name))
            //    {
            //        string path = file.FullName;

            //        try
            //        {
            //            var stream = client.OpenRead(path);

            //            queue.Add(new FileItem(client, path, stream));
            //        }
            //        catch (FtpException) { }
            //    }
            //}
        }

        #endregion
    }
    internal class FtpAdapterInboundReply : InboundReply
    {
        private FtpClient client;
        private Stream stream;
        private string path;

        public FtpAdapterInboundReply(FtpClient client, string path, Stream stream)
        {
            this.client = client;
            this.path = path;
            this.stream = stream;
        }

        #region InboundReply Members

        /// <summary>
        /// Abort the inbound reply call
        /// </summary>
        public override void Abort()
        {
            stream.Close();
        }

        /// <summary>
        /// Reply message implemented
        /// </summary>
        public override void Reply(System.ServiceModel.Channels.Message message
            , TimeSpan timeout)
        {
            if (!message.IsFault)
            {
                client.DeleteFile(path);
            }
            stream.Close();
        }
        
        #endregion InboundReply Members
    }

}
