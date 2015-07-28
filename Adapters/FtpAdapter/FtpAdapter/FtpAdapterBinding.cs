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
/// Module      :  FtpAdapterBinding.cs
/// Description :  This is the class used while creating a binding for an adapter
/// -----------------------------------------------------------------------------------------------------------

#region Using Directives
using System;
using System.Collections.Generic;
using System.Text;
using System.ServiceModel;
using System.ServiceModel.Channels;
using System.ServiceModel.Configuration;

using Microsoft.ServiceModel.Channels.Common;
#endregion

namespace Reply.Cluster.Mercury.Adapters.Ftp
{
    public class FtpAdapterBinding : AdapterBinding
    {
        // Scheme in Binding does not have to be the same as Adapter Scheme. 
        // Over write this value as appropriate.
        private const string BindingScheme = "ftp";

        /// <summary>
        /// Initializes a new instance of the AdapterBinding class
        /// </summary>
        public FtpAdapterBinding() { }

        /// <summary>
        /// Initializes a new instance of the AdapterBinding class with a configuration name
        /// </summary>
        public FtpAdapterBinding(string configName)
        {
            ApplyConfiguration(configName);
        }

        /// <summary>
        /// Applies the current configuration to the FtpAdapterBindingCollectionElement
        /// </summary>
        private void ApplyConfiguration(string configurationName)
        {
            BindingsSection bindingsSection = (BindingsSection)System.Configuration.ConfigurationManager.GetSection("system.serviceModel/bindings");
            FtpAdapterBindingCollectionElement bindingCollectionElement = (FtpAdapterBindingCollectionElement)bindingsSection["ftpBinding"];
            FtpAdapterBindingElement element = bindingCollectionElement.Bindings[configurationName];
            if (element != null)
            {
                element.ApplyConfiguration(this);
            }
        }



        #region Private Fields

        private FtpAdapter binding;

        #endregion Private Fields

        #region Custom Generated Fields

        private PollingType pollingType;


        private int pollingInterval;


        private string scheduleName;


        private string tempFolder;


        private string remoteBackup;


        private string localBackup;


        private OverwriteAction overwriteAction;


        private bool zipFile;

        private string subFolders;

        #endregion Custom Generated Fields

        #region Public Properties

        /// <summary>
        /// Gets the URI transport scheme that is used by the channel and listener factories that are built by the bindings.
        /// </summary>
        public override string Scheme
        {
            get
            {
                return BindingScheme;
            }
        }

        /// <summary>
        /// Returns a value indicating whether this binding supports metadata browsing.
        /// </summary>
        public override bool SupportsMetadataBrowse
        {
            get
            {
                return false;
            }
        }

        /// <summary>
        /// Returns a value indicating whether this binding supports metadata retrieval.
        /// </summary>
        public override bool SupportsMetadataGet
        {
            get
            {
                return false;
            }
        }

        /// <summary>
        /// Returns a value indicating whether this binding supports metadata searching.
        /// </summary>
        public override bool SupportsMetadataSearch
        {
            get
            {
                return false;
            }
        }

        /// <summary>
        /// Returns the custom type of the ConnectionUri.
        /// </summary>
        public override Type ConnectionUriType
        {
            get
            {
                return typeof(FtpAdapterConnectionUri);
            }
        }

        #endregion Public Properties

        #region Custom Generated Properties

        [System.Configuration.ConfigurationProperty("pollingType", DefaultValue = PollingType.Simple)]
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



        [System.Configuration.ConfigurationProperty("zipFile", DefaultValue = false)]
        public bool ZipFile
        {
            get
            {
                return this.zipFile;
            }
            set
            {
                this.zipFile = value;
            }
        }

        [System.Configuration.ConfigurationProperty("SubFolders")]
        public string SubFolders
        {
            get
            {
                return this.subFolders;
            }
            set
            {
                this.subFolders = value;
            }
        }

        #endregion Custom Generated Properties

        #region Private Properties

        private FtpAdapter BindingElement
        {
            get
            {
                if (binding == null)
                    binding = new FtpAdapter();
                binding.PollingType = this.PollingType;
                binding.PollingInterval = this.PollingInterval;
                binding.ScheduleName = this.ScheduleName;
                binding.TempFolder = this.TempFolder;
                binding.RemoteBackup = this.RemoteBackup;
                binding.LocalBackup = this.LocalBackup;
                binding.OverwriteAction = this.OverwriteAction;
                binding.ZipFile = this.ZipFile;
                binding.SubFolders = this.SubFolders;
                return binding;
            }
        }

        #endregion Private Properties

        #region Public Methods

        /// <summary>
        /// Creates a clone of the existing BindingElement and returns it
        /// </summary>
        public override BindingElementCollection CreateBindingElements()
        {
            BindingElementCollection bindingElements = new BindingElementCollection();
            //Only create once
            bindingElements.Add(this.BindingElement);
            //Return the clone
            return bindingElements.Clone();
        }

        #endregion Public Methods

    }
}
