using System;
using System.ComponentModel;
using System.Configuration;

namespace QueueIT.Security.Configuration
{
    public class SettingsSection : ConfigurationSection
    {
        public const string ConfigurationSectionName = "queueit.security";

        [ConfigurationProperty("secretKey", DefaultValue = null, IsRequired = true)]
        public virtual string SecretKey
        {
            get
            {
                return (string)this["secretKey"];
            }
            set
            {
                this["secretKey"] = value;
            }
        }

        [ConfigurationProperty("queryStringPrefix", DefaultValue = null, IsRequired = false)]
        public virtual string QueryStringPrefix
        {
            get
            {
                return (string)this["queryStringPrefix"];
            }
            set
            {
                this["queryStringPrefix"] = value;
            }
        }

        [ConfigurationProperty("ticketExpiration", DefaultValue = null, IsRequired = false)]
        [TypeConverter(typeof(TimeSpanConverter))]
        public virtual TimeSpan TicketExpiration
        {
            get
            {
                return (TimeSpan)this["ticketExpiration"];
            }
            set
            {
                this["ticketExpiration"] = value;
            }
        }

        [ConfigurationProperty("queues", IsRequired = true, IsKey = false, IsDefaultCollection = true)]
        public QueueCollection Queues
        {
            get { return ((QueueCollection)(base["queues"])); }
            set { base["queues"] = value; }
        }

        [ConfigurationProperty("repositorySettings", IsRequired = false, IsKey = false, IsDefaultCollection = true)]
        public RepositorySettingsCollection RepositorySettings
        {
            get { return ((RepositorySettingsCollection)(base["repositorySettings"])); }
            set { base["repositorySettings"] = value; }
        }

        public static SettingsSection GetSection()
        {
            return ConfigurationManager.GetSection(ConfigurationSectionName) as SettingsSection;
        }
    }
}
