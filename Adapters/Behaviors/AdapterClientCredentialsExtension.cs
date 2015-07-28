using System;
using System.Collections.Generic;
using System.Linq;
using System.ServiceModel.Configuration;
using System.Text;
using System.Threading.Tasks;

namespace Reply.Cluster.Mercury.Adapters.Behaviors
{
    public class AdapterClientCredentialsExtension : BehaviorExtensionElement
    {
        [System.ComponentModel.Category("Credentials")]
        [System.Configuration.ConfigurationProperty("UserName", DefaultValue = "anonymous")]
        public string UserName
        {
            get
            {
                return ((string)(base["UserName"]));
            }
            set
            {
                base["UserName"] = value;
            }
        }

        [System.ComponentModel.Category("Credentials")]
        [System.Configuration.ConfigurationProperty("Password", DefaultValue = "")]
        public string Password
        {
            get
            {
                return ((string)(base["Password"]));
            }
            set
            {
                base["Password"] = value;
            }
        }


        protected override object CreateBehavior()
        {
            System.Diagnostics.Debugger.Launch();
            var behavior = new AdapterClientCredentials();
            behavior.UserName.UserName = this.UserName;
            behavior.UserName.Password = this.Password;

            return behavior;
        }

        public override Type BehaviorType
        {
            get { return typeof(AdapterClientCredentials); }
        }
    }
}
