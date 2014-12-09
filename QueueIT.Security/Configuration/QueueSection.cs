using System;
using System.ComponentModel;
using System.Configuration;

namespace QueueIT.Security.Configuration
{
    public class QueueSection : ConfigurationElement
    {
        [ConfigurationProperty("name", IsKey = true, IsRequired = true)]
        public virtual string Name
        {
            get
            {
                return (string)this["name"];
            }
            set
            {
                this["name"] = value;
            }
        }

        [ConfigurationProperty("customerId", IsRequired = true)]
        public virtual string CustomerId
        {
            get
            {
                return (string)this["customerId"];
            }
            set
            {
                this["customerId"] = value;
            }
        }

        [ConfigurationProperty("eventId", IsRequired = true)]
        public virtual string EventId
        {
            get
            {
                return (string)this["eventId"];
            }
            set
            {
                this["eventId"] = value;
            }
        }

        [ConfigurationProperty("domainAlias", DefaultValue = null, IsRequired = false)]
        public virtual string DomainAlias
        {
            get
            {
                return (string)this["domainAlias"];
            }
            set
            {
                this["domainAlias"] = value;
            }
        }

        [ConfigurationProperty("landingPage", DefaultValue = null, IsRequired = false)]
        public virtual string LandingPage
        {
            get
            {
                return (string)this["landingPage"];
            }
            set
            {
                this["landingPage"] = value;
            }
        }

        [ConfigurationProperty("useSsl", DefaultValue = false, IsRequired = false)]
        public virtual bool UseSsl
        {
            get
            {
                return (bool)this["useSsl"];
            }
            set
            {
                this["useSsl"] = value;
            }
        }

        [ConfigurationProperty("includeTargetUrl", DefaultValue = false, IsRequired = false)]
        public virtual bool IncludeTargetUrl
        {
            get
            {
                return (bool)this["includeTargetUrl"];
            }
            set
            {
                this["includeTargetUrl"] = value;
            }
        }

        [ConfigurationProperty("language", DefaultValue = null, IsRequired = false)]
        public virtual string Language
        {
            get
            {
                return (string)this["language"];
            }
            set
            {
                this["language"] = value;
            }
        }

        [ConfigurationProperty("layoutName", DefaultValue = null, IsRequired = false)]
        public virtual string LayoutName
        {
            get
            {
                return (string)this["layoutName"];
            }
            set
            {
                this["layoutName"] = value;
            }
        }

    }
}
