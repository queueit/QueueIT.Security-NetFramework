using System;
using System.Collections.Generic;
using System.Configuration;
using System.Globalization;
using System.Linq;
using QueueIT.Security.Configuration;

namespace QueueIT.Security
{
    /// <summary>
    /// Provides factory methods to create IQueue instances
    /// </summary>
    /// <remarks>
    /// View members for additional information and examples
    /// </remarks>
    public static class QueueFactory
    {
        private static string _hostDomain = "queue-it.net";
        private static SettingsSection _settings;

        private static Dictionary<string, Queue> _queues = new Dictionary<string, Queue>(); 

        static QueueFactory()
        {
            _settings = SettingsSection.GetSection();
        }

        internal static void Reset()
        {
            _queues = new Dictionary<string, Queue>();

            _hostDomain = "queue-it.net";

        }

        /// <summary>
        /// Creates or gets the default queue defined by configuration 
        /// This method requires a queue with then name 'default' to be configured in the application config file
        /// </summary>
        /// <returns>The IQueue singleton object</returns>
        /// <example>
        /// Source Code;
        /// <code language="cs">
        /// IQueue defaultQueue = QueueFactory.CreateQueue();
        /// </code>
        /// 
        /// Configuration:
        /// <code>
        /// <![CDATA[
        /// <configuration>
        ///    <configSections>
        ///       <section name="queueit.security" type="QueueIT.Security.Configuration.SettingsSection, QueueIT.Security"/>
        ///    </configSections>
        ///    <queueit.security 
        ///       secretKey="a774b1e2-8da7-4d51-b1a9-7647147bb13bace77210-a488-4b6f-afc9-8ba94551a7d7">
        ///       <queues>
        ///          <queue name="default" customerId="ticketania" eventId="simple"/>
        ///       </queues>
        ///    </queueit.security>
        /// </configuration>
        /// ]]>
        /// </code>
        /// </example>
        public static IQueue CreateQueue()
        {
            return CreateQueue("default");
        }

        /// <summary>
        /// Creates or gets a queue defined by configuration 
        /// This method requires a queue to be configured in the application config file with the name provided in queueName
        /// </summary>
        /// <param name="queueName">The name of the queue as defined in the configuration file</param>
        /// <returns>The IQueue singleton object</returns>
        /// <example>
        /// Source Code;
        /// <code language="cs">
        /// IQueue defaultQueue = QueueFactory.CreateQueue("myqueue");
        /// </code>
        /// 
        /// Configuration:
        /// <code>
        /// <![CDATA[
        /// <configuration>
        ///    <configSections>
        ///       <section name="queueit.security" type="QueueIT.Security.Configuration.SettingsSection, QueueIT.Security"/>
        ///    </configSections>
        ///    <queueit.security 
        ///       secretKey="a774b1e2-8da7-4d51-b1a9-7647147bb13bace77210-a488-4b6f-afc9-8ba94551a7d7">
        ///       <queues>
        ///          <queue name="myqueue" customerId="ticketania" eventId="advanced"/>
        ///       </queues>
        ///    </queueit.security>
        /// </configuration>
        /// ]]>
        /// </code>
        /// </example>
        public static IQueue CreateQueue(string queueName)
        {
            if (string.IsNullOrEmpty(queueName))
                throw new ArgumentException("Queue Name cannot be null or empty", "queueName");

            if (_settings == null)
                throw new ConfigurationErrorsException(string.Format(
                    "Configuration section '{0}' is missing from configuration file",
                    SettingsSection.ConfigurationSectionName));

            QueueSection queueConfiguration = _settings.Queues == null 
                ? null 
                :_settings.Queues.GetGeneric().FirstOrDefault(queueConfig => queueConfig.Name == queueName);

            if (queueConfiguration == null)
                throw new ConfigurationErrorsException(string.Format(
                    "Configuration for Queue Name '{0}' in section '{1}' is missing from configuration file",
                    queueName,
                    SettingsSection.ConfigurationSectionName));

            Queue queue = InstantiateQueue(
                queueConfiguration.CustomerId, 
                queueConfiguration.EventId, 
                queueConfiguration.DomainAlias,
                queueConfiguration.LandingPage,
                queueConfiguration.UseSsl,
                queueConfiguration.IncludeTargetUrl,
                string.IsNullOrEmpty(queueConfiguration.Language) ? null : new CultureInfo(queueConfiguration.Language),
                queueConfiguration.LayoutName);

            return queue;
        }

        /// <summary>
        /// Creates or gets a queue not using configuration 
        /// </summary>
        /// <param name="customerId">The Customer ID of the queue</param>
        /// <param name="eventId">The Event ID of the queue</param>
        /// <returns>The IQueue singleton object</returns>
        /// <example>
        /// Source Code;
        /// <code language="cs">
        /// IQueue defaultQueue = QueueFactory.CreateQueue("ticketania", "codeonly");
        /// </code>
        /// </example>
        public static IQueue CreateQueue(string customerId, string eventId)
        {
            if (string.IsNullOrEmpty(customerId))
                throw new ArgumentException("Customer ID cannot be null or empty", "customerId");
            if (string.IsNullOrEmpty(eventId))
                throw new ArgumentException("Event ID cannot be null or empty", "eventId");

            var queue = InstantiateQueue(customerId, eventId, null, null, false, false, null, null);

            return queue;
        }

        private static Queue InstantiateQueue(string customerId, string eventId, string domainAlias, Uri landingPage, 
            bool sslEnabled, bool includeTargetUrl, CultureInfo culture, string layoutName)
        {
            string key = GenerateKey(customerId, eventId);

            Dictionary<string, Queue> queues = _queues;

            if (queues.ContainsKey(key))
                return queues[key];

            if (string.IsNullOrEmpty(domainAlias))
            {
                domainAlias = string.Format(
                    "{0}-{1}.{2}",
                    eventId,
                    customerId,
                    _hostDomain);
            }

            Queue queue = new Queue(
                customerId,
                eventId,
                domainAlias,
                landingPage,
                sslEnabled, 
                includeTargetUrl,
                culture, 
                layoutName);
            _queues.Add(key, queue);

            return queue;
        }

        /// <summary>
        /// Configure the Queue Factory
        /// </summary>
        /// <param name="hostDomain">The domain name of the Queue-it service. This should not normally be set</param>
        public static void Configure(string hostDomain = "queue-it.net")
        {
            _hostDomain = hostDomain;
        }

        private static string GenerateKey(string customerId, string eventId)
        {
            return string.Concat(customerId, "_", eventId);
        }
    }
}
