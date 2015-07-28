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
/// Module      :  FileAdapter.cs
/// Description :  The main adapter class which inherits from Adapter
/// -----------------------------------------------------------------------------------------------------------

#region Using Directives
using System;
using System.Collections.Generic;
using System.Text;
using System.ServiceModel.Description;

using Microsoft.ServiceModel.Channels.Common;
#endregion

namespace Reply.Cluster.Mercury.Adapters.File
{
    public enum PollingType
    {
        Event,
        Simple,
        Managed
    }

    public enum OverwriteAction
    {
        None,
        Replace,
        Append
    }

    public class FileAdapter : Adapter
    {
        // Scheme associated with the adapter
        internal const string SCHEME = "file";
        // Namespace for the proxy that will be generated from the adapter schema
        internal const string SERVICENAMESPACE = "http://mercury.cluster.reply.eu/adapters/file/2014/05";
        // Initializes the AdapterEnvironmentSettings class
        private static AdapterEnvironmentSettings environmentSettings = new AdapterEnvironmentSettings();

        #region Custom Generated Fields

        private PollingType pollingType;


        private int pollingInterval;


        private string scheduleName;


        private string tempFolder;


        private string remoteBackup;


        private string localBackup;


        private OverwriteAction overwriteAction;

        private bool includeSubfolders;

        #endregion Custom Generated Fields

        #region  Constructor

        /// <summary>
        /// Initializes a new instance of the FileAdapter class
        /// </summary>
        public FileAdapter()
            : base(environmentSettings)
        {
            Settings.Metadata.DefaultMetadataNamespace = SERVICENAMESPACE;
        }

        /// <summary>
        /// Initializes a new instance of the FileAdapter class with a binding
        /// </summary>
        public FileAdapter(FileAdapter binding)
            : base(binding)
        {
            this.PollingType = binding.PollingType;
            this.PollingInterval = binding.PollingInterval;
            this.ScheduleName = binding.ScheduleName;
            this.TempFolder = binding.TempFolder;
            this.RemoteBackup = binding.RemoteBackup;
            this.LocalBackup = binding.LocalBackup;
            this.OverwriteAction = binding.OverwriteAction;
            this.IncludeSubfolders = binding.IncludeSubfolders;
        }

        #endregion Constructor

        #region Custom Generated Properties

        [System.ComponentModel.Category("Polling")]
        [System.Configuration.ConfigurationProperty("pollingType", DefaultValue = PollingType.Event)]
        public PollingType PollingType
        {
            get
            {
                return this.pollingType;
            }
            set
            {
                this.pollingType = value;
            }
        }


        [System.ComponentModel.Category("Polling")]
        [System.Configuration.ConfigurationProperty("pollingInterval", DefaultValue = 60)]
        public int PollingInterval
        {
            get
            {
                return this.pollingInterval;
            }
            set
            {
                this.pollingInterval = value;
            }
        }


        [System.ComponentModel.Category("Polling")]
        [System.Configuration.ConfigurationProperty("ScheduleName")]
        public string ScheduleName
        {
            get
            {
                return this.scheduleName;
            }
            set
            {
                this.scheduleName = value;
            }
        }


        [System.ComponentModel.Category("Folders")]
        [System.Configuration.ConfigurationProperty("TempFolder")]
        public string TempFolder
        {
            get
            {
                return this.tempFolder;
            }
            set
            {
                this.tempFolder = value;
            }
        }


        [System.ComponentModel.Category("Folders")]
        [System.Configuration.ConfigurationProperty("RemoteBackup")]
        public string RemoteBackup
        {
            get
            {
                return this.remoteBackup;
            }
            set
            {
                this.remoteBackup = value;
            }
        }


        [System.ComponentModel.Category("Folders")]
        [System.Configuration.ConfigurationProperty("LocalBackup")]
        public string LocalBackup
        {
            get
            {
                return this.localBackup;
            }
            set
            {
                this.localBackup = value;
            }
        }


        [System.ComponentModel.Category("Overwrite")]
        [System.Configuration.ConfigurationProperty("overwriteAction", DefaultValue = OverwriteAction.None)]
        public OverwriteAction OverwriteAction
        {
            get
            {
                return this.overwriteAction;
            }
            set
            {
                this.overwriteAction = value;
            }
        }

        [System.ComponentModel.Category("Folders")]
        [System.Configuration.ConfigurationProperty("includeSubfolders", DefaultValue = false)]
        public bool IncludeSubfolders
        {
            get
            {
                return this.includeSubfolders;
            }
            set
            {
                this.includeSubfolders = value;
            }
        }

        #endregion Custom Generated Properties

        #region Public Properties

        /// <summary>
        /// Gets the URI transport scheme that is used by the adapter
        /// </summary>
        public override string Scheme
        {
            get
            {
                return SCHEME;
            }
        }

        #endregion Public Properties

        #region Protected Methods

        /// <summary>
        /// Creates a ConnectionUri instance from the provided Uri
        /// </summary>
        protected override ConnectionUri BuildConnectionUri(Uri uri)
        {
            return new FileAdapterConnectionUri(uri);
        }

        /// <summary>
        /// Builds a connection factory from the ConnectionUri and ClientCredentials
        /// </summary>
        protected override IConnectionFactory BuildConnectionFactory(
            ConnectionUri connectionUri
            , ClientCredentials clientCredentials
            , System.ServiceModel.Channels.BindingContext context)
        {
            return new FileAdapterConnectionFactory(connectionUri, clientCredentials, this);
        }

        /// <summary>
        /// Returns a clone of the adapter object
        /// </summary>
        protected override Adapter CloneAdapter()
        {
            return new FileAdapter(this);
        }

        /// <summary>
        /// Indicates whether the provided TConnectionHandler is supported by the adapter or not
        /// </summary>
        protected override bool IsHandlerSupported<TConnectionHandler>()
        {
            return (
                  typeof(IOutboundHandler) == typeof(TConnectionHandler)
                || typeof(IInboundHandler) == typeof(TConnectionHandler));
        }

        /// <summary>
        /// Gets the namespace that is used when generating schema and WSDL
        /// </summary>
        protected override string Namespace
        {
            get
            {
                return SERVICENAMESPACE;
            }
        }

        #endregion Protected Methods
    }
}
