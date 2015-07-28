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
/// Module      :  FtpAdapterBindingElementExtensionElement.cs
/// Description :  This class is provided to surface Adapter as a binding element, so that it 
///                can be used within a user-defined WCF "Custom Binding".
///                In configuration file, it is defined under
///                <system.serviceModel>
///                  <extensions>
///                     <bindingElementExtensions>
///                         <add name="{name}" type="{this}, {assembly}"/>
///                     </bindingElementExtensions>
///                  </extensions>
///                </system.serviceModel>
/// -----------------------------------------------------------------------------------------------------------

#region Using Directives
using System;
using System.Collections.Generic;
using System.Text;
using System.ServiceModel;
using System.ServiceModel.Configuration;
using System.ServiceModel.Channels;
using System.Configuration;
using System.Globalization;

using Microsoft.ServiceModel.Channels.Common;
#endregion

namespace Reply.Cluster.Mercury.Adapters.Ftp
{
    using System;
    using System.Configuration;
    using System.ServiceModel;
    using System.ServiceModel.Channels;
    using System.ServiceModel.Configuration;

    public class FtpAdapterBindingElementExtensionElement : BindingElementExtensionElement
    {

        #region  Constructor

        /// <summary>
        /// Default constructor
        /// </summary>
        public FtpAdapterBindingElementExtensionElement()
        {
        }

        #endregion Constructor

        #region Custom Generated Properties

        [System.ComponentModel.Category("Polling")]
        [System.Configuration.ConfigurationProperty("pollingType", DefaultValue = PollingType.Simple)]
        public PollingType PollingType
        {
            get
            {
                return ((PollingType)(base["PollingType"]));
            }
            set
            {
                base["PollingType"] = value;
            }
        }


        [System.ComponentModel.Category("Polling")]
        [System.Configuration.ConfigurationProperty("pollingInterval", DefaultValue = 60)]
        public int PollingInterval
        {
            get
            {
                return ((int)(base["PollingInterval"]));
            }
            set
            {
                base["PollingInterval"] = value;
            }
        }


        [System.ComponentModel.Category("Polling")]
        [System.Configuration.ConfigurationProperty("ScheduleName")]
        public string ScheduleName
        {
            get
            {
                return ((string)(base["ScheduleName"]));
            }
            set
            {
                base["ScheduleName"] = value;
            }
        }


        [System.ComponentModel.Category("Folders")]
        [System.Configuration.ConfigurationProperty("TempFolder")]
        public string TempFolder
        {
            get
            {
                return ((string)(base["TempFolder"]));
            }
            set
            {
                base["TempFolder"] = value;
            }
        }


        [System.ComponentModel.Category("Folders")]
        [System.Configuration.ConfigurationProperty("RemoteBackup")]
        public string RemoteBackup
        {
            get
            {
                return ((string)(base["RemoteBackup"]));
            }
            set
            {
                base["RemoteBackup"] = value;
            }
        }


        [System.ComponentModel.Category("Folders")]
        [System.Configuration.ConfigurationProperty("LocalBackup")]
        public string LocalBackup
        {
            get
            {
                return ((string)(base["LocalBackup"]));
            }
            set
            {
                base["LocalBackup"] = value;
            }
        }


        [System.ComponentModel.Category("Overwrite")]
        [System.Configuration.ConfigurationProperty("overwriteAction", DefaultValue = OverwriteAction.None)]
        public OverwriteAction OverwriteAction
        {
            get
            {
                return ((OverwriteAction)(base["OverwriteAction"]));
            }
            set
            {
                base["OverwriteAction"] = value;
            }
        }


        [System.ComponentModel.Category("Compression")]
        [System.Configuration.ConfigurationProperty("zipFile", DefaultValue = false)]
        public bool ZipFile
        {
            get
            {
                return ((bool)(base["ZipFile"]));
            }
            set
            {
                base["ZipFile"] = value;
            }
        }

        [System.ComponentModel.Category("Folder")]
        [System.Configuration.ConfigurationProperty("SubFolders", DefaultValue = "")]
        public string SubFolders
        {
            get
            {
                return ((string)(base["SubFolders"]));
            }
            set
            {
                base["SubFolders"] = value;
            }
        }

        #endregion Custom Generated Properties

        #region BindingElementExtensionElement Methods
        /// <summary>
        /// Return the type of the adapter (binding element)
        /// </summary>
        public override Type BindingElementType
        {
            get
            {
                return typeof(FtpAdapter);
            }
        }
        /// <summary>
        /// Returns a collection of the configuration properties
        /// </summary>
        protected override ConfigurationPropertyCollection Properties
        {
            get
            {
                ConfigurationPropertyCollection configProperties = base.Properties;
                configProperties.Add(new ConfigurationProperty("PollingType", typeof(PollingType), PollingType.Simple, null, null, ConfigurationPropertyOptions.None));
                configProperties.Add(new ConfigurationProperty("PollingInterval", typeof(System.Int32), (System.Int32)60, null, null, ConfigurationPropertyOptions.None));
                configProperties.Add(new ConfigurationProperty("ScheduleName", typeof(System.String), null, null, null, ConfigurationPropertyOptions.None));
                configProperties.Add(new ConfigurationProperty("TempFolder", typeof(System.String), null, null, null, ConfigurationPropertyOptions.None));
                configProperties.Add(new ConfigurationProperty("RemoteBackup", typeof(System.String), null, null, null, ConfigurationPropertyOptions.None));
                configProperties.Add(new ConfigurationProperty("LocalBackup", typeof(System.String), null, null, null, ConfigurationPropertyOptions.None));
                configProperties.Add(new ConfigurationProperty("OverwriteAction", typeof(OverwriteAction), OverwriteAction.None, null, null, ConfigurationPropertyOptions.None));
                configProperties.Add(new ConfigurationProperty("ZipFile", typeof(System.Boolean), false, null, null, ConfigurationPropertyOptions.None));
                return configProperties;
            }
        }

        /// <summary>
        /// Instantiate the adapter.
        /// </summary>
        /// <returns></returns>
        protected override BindingElement CreateBindingElement()
        {
            FtpAdapter adapter = new FtpAdapter();
            this.ApplyConfiguration(adapter);
            return adapter;
        }

        /// <summary>
        /// Apply the configuration properties to the adapter.
        /// </summary>
        /// <param name="bindingElement"></param>
        public override void ApplyConfiguration(BindingElement bindingElement)
        {
            base.ApplyConfiguration(bindingElement);
            FtpAdapter adapterBinding = ((FtpAdapter)(bindingElement));
            adapterBinding.PollingType = (PollingType)this["PollingType"];
            adapterBinding.PollingInterval = (System.Int32)this["PollingInterval"];
            adapterBinding.ScheduleName = (System.String)this["ScheduleName"];
            adapterBinding.TempFolder = (System.String)this["TempFolder"];
            adapterBinding.RemoteBackup = (System.String)this["RemoteBackup"];
            adapterBinding.LocalBackup = (System.String)this["LocalBackup"];
            adapterBinding.OverwriteAction = (OverwriteAction)this["OverwriteAction"];
            adapterBinding.ZipFile = (System.Boolean)this["ZipFile"];
            adapterBinding.SubFolders = (System.String)this["SubFolders"];
        }

        /// <summary>
        /// Initialize the binding properties from the adapter.
        /// </summary>
        /// <param name="bindingElement"></param>
        protected override void InitializeFrom(BindingElement bindingElement)
        {
            base.InitializeFrom(bindingElement);
            FtpAdapter adapterBinding = ((FtpAdapter)(bindingElement));
            this["PollingType"] = adapterBinding.PollingType;
            this["PollingInterval"] = adapterBinding.PollingInterval;
            this["ScheduleName"] = adapterBinding.ScheduleName;
            this["TempFolder"] = adapterBinding.TempFolder;
            this["RemoteBackup"] = adapterBinding.RemoteBackup;
            this["LocalBackup"] = adapterBinding.LocalBackup;
            this["OverwriteAction"] = adapterBinding.OverwriteAction;
            this["ZipFile"] = adapterBinding.ZipFile;
            this["SubFolders"] = adapterBinding.SubFolders;
        }

        /// <summary>
        /// Copy the properties to the custom binding
        /// </summary>
        /// <param name="from"></param>
        public override void CopyFrom(ServiceModelExtensionElement from)
        {
            base.CopyFrom(from);
            FtpAdapterBindingElementExtensionElement adapterBinding = ((FtpAdapterBindingElementExtensionElement)(from));
            this["PollingType"] = adapterBinding.PollingType;
            this["PollingInterval"] = adapterBinding.PollingInterval;
            this["ScheduleName"] = adapterBinding.ScheduleName;
            this["TempFolder"] = adapterBinding.TempFolder;
            this["RemoteBackup"] = adapterBinding.RemoteBackup;
            this["LocalBackup"] = adapterBinding.LocalBackup;
            this["OverwriteAction"] = adapterBinding.OverwriteAction;
            this["ZipFile"] = adapterBinding.ZipFile;
            this["SubFolders"] = adapterBinding.SubFolders;
        }

        #endregion BindingElementExtensionElement Methods
    }
}

