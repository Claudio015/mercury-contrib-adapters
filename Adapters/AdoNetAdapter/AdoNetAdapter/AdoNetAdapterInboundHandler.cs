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
/// Module      :  AdoNetAdapterInboundHandler.cs
/// Description :  This class implements an interface for listening or polling for data.
/// -----------------------------------------------------------------------------------------------------------
/// 
#region Using Directives
using System;
using System.Collections.Generic;
using System.Text;

using Microsoft.ServiceModel.Channels.Common;
using System.ServiceModel.Channels;
using System.Collections.Concurrent;
using System.Threading;
using System.Xml;
using System.Data.Common;
using System.Transactions;
using Reply.Cluster.Mercury.Adapters.Helpers;
#endregion

namespace Reply.Cluster.Mercury.Adapters.AdoNet
{
    public class AdoNetAdapterInboundHandler : AdoNetAdapterHandlerBase, IInboundHandler
    {
        /// <summary>
        /// Initializes a new instance of the AdoNetAdapterInboundHandler class
        /// </summary>
        public AdoNetAdapterInboundHandler(AdoNetAdapterConnection connection
            , MetadataLookup metadataLookup)
            : base(connection, metadataLookup)
        {
            pollingType = connection.ConnectionFactory.Adapter.PollingType;

            if (pollingType == PollingType.Simple)
            {
                pollingInterval = connection.ConnectionFactory.Adapter.PollingInterval * 1000;

                pollingTimer = new Timer(new TimerCallback(t => ExecutePolling()));
            }
            else
            {
                scheduleName = connection.ConnectionFactory.Adapter.ScheduleName;
            }

            UriBuilder actionBuilder = new UriBuilder(AdoNetAdapter.SERVICENAMESPACE);
            actionBuilder.Path = System.IO.Path.Combine(actionBuilder.Path, connection.ConnectionFactory.ConnectionUri.ConnectionName);
            actionBuilder.Path = System.IO.Path.Combine(actionBuilder.Path, connection.ConnectionFactory.ConnectionUri.InboundID);

            action = actionBuilder.ToString() + "#Receive";
        }

        #region Private Fields

        private string action;
        private PollingType pollingType;

        private Timer pollingTimer;
        private int pollingInterval;
        private string scheduleName;

        private BlockingCollection<MessageItem> queue = new BlockingCollection<MessageItem>();
        private CancellationTokenSource cancelSource = new CancellationTokenSource();

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
                ScheduleHelper.RegisterEvent(scheduleName, () => ExecutePolling());
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
            
            MessageItem item = null;
            bool result = queue.TryTake(out item, (int)Math.Min(timeout.TotalMilliseconds, (long)int.MaxValue), cancelSource.Token);

            if (result)
            {
                message = item.Message;
                reply = new AdoNetAdapterInboundReply(item.Connection, item.Transaction, item.EndOperationStatement);
            }

            return result;
        }

        /// <summary>
        /// Returns a value that indicates whether a message has arrived within a specified interval of time.
        /// </summary>
        public bool WaitForMessage(TimeSpan timeout)
        {
            // TODO: vedere se fare una logica più sensata

            return !queue.IsCompleted;
        }

        #endregion IInboundHandler Members

        #region Event Handlers

        private void ExecutePolling()
        {
            string dataAvailableStatement = Connection.ConnectionFactory.Adapter.DataAvailableStatement;
            string getDataStatement = Connection.ConnectionFactory.Adapter.GetDataStatement;
            string endOperationStatement = Connection.ConnectionFactory.Adapter.EndOperationStatement;

            bool dataAvailable = true;

            if (!string.IsNullOrWhiteSpace(dataAvailableStatement))
            {
                using (var connection = Connection.CreateDbConnection())
                {
                    connection.Open();

                    var dataAvailableCommand = connection.CreateCommand();
                    dataAvailableCommand.CommandText = dataAvailableStatement;

                    int? count = dataAvailableCommand.ExecuteScalar() as int?;
                    dataAvailable = count.HasValue && (count > 0);
                }
            }

            if (dataAvailable)
            {
                bool goOn = Connection.ConnectionFactory.Adapter.PollWhileDataFound;

                do
                {
                    if (queue.IsAddingCompleted)
                        return;

                    var connection = Connection.CreateDbConnection();
                    connection.Open();

                    var transaction = new CommittableTransaction(new TransactionOptions { IsolationLevel = Connection.ConnectionFactory.Adapter.IsolationLevel });
                    connection.EnlistTransaction(transaction);
                    
                    var getDataCommand = connection.CreateCommand();
                    getDataCommand.CommandText = getDataStatement;

                    using (var reader = getDataCommand.ExecuteReader())
                    {
                        if (reader.HasRows)
                        {
                            queue.Add(new MessageItem
                            {
                                Message = DbHelpers.CreateMessage(reader, Connection.ConnectionFactory.Adapter.UseAmbientTransaction ? transaction : null, action),
                                Connection = connection,
                                Transaction = transaction,
                                EndOperationStatement = endOperationStatement
                            });
                        }
                        else
                        {
                            transaction.Commit();
                            connection.Close();

                            goOn = false;
                        }
                    }
                } while (goOn);
            }
        }

        #endregion Event Handlers

        #region IDisposable Members

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                pollingTimer.Dispose();
            }
        }

        #endregion IDisposable Members
    }
    internal class AdoNetAdapterInboundReply : InboundReply
    {
        private DbConnection connection;
        private CommittableTransaction transaction;
        private string endOperationStatement;

        public AdoNetAdapterInboundReply(DbConnection connection, CommittableTransaction transaction, string endOperationStatement)
        {
            this.connection = connection;
            this.transaction = transaction;
            this.endOperationStatement = endOperationStatement;
        }

        #region InboundReply Members

        /// <summary>
        /// Abort the inbound reply call
        /// </summary>
        public override void Abort()
        {
            try
            {
                transaction.Rollback();
                connection.Close();
            }
            catch { }
        }

        /// <summary>
        /// Reply message implemented
        /// </summary>
        public override void Reply(System.ServiceModel.Channels.Message message
            , TimeSpan timeout)
        {
            if (!string.IsNullOrWhiteSpace(endOperationStatement))
            {
                var endOperationCommand = connection.CreateCommand();
                endOperationCommand.CommandText = endOperationStatement;

                endOperationCommand.ExecuteNonQuery();
            }

            transaction.Commit();
            connection.Close();
        }


        #endregion InboundReply Members
    }

    internal class MessageItem
    {
        public Message Message { get; set; }

        public DbConnection Connection { get; set; }
        public CommittableTransaction Transaction { get; set; }
        public string EndOperationStatement { get; set; }
    }
}
