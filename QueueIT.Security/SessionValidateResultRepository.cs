using System;
using System.Web;
using System.Web.SessionState;
using QueueIT.Security.Configuration;

namespace QueueIT.Security
{
    /// <summary>
    /// A repository to store state which stores the validation result in the built in session state.
    /// </summary>
    /// <remarks>
    /// The default repository is the CookieValidateResultRepository. Use the Configure method of the SessionValidationController to override.<br /><br />
    /// NOTE: Users will be looped back to the queue if their browser does not support cookies. 
    /// It is highly recommended that your application confirms that there is cookie support before sending the user to the queue. 
    /// If there is no cookie support we recommend displaying an error message to the user asking them to enable cookies.
    /// </remarks>
    /// <example>
    /// <code language="cs">
    /// SessionValidationController.Configure(validationResultProviderFactory: () => new SessionValidateResultRepository);
    /// </code>
    /// Configuration:
    /// <code>
    /// <![CDATA[
    /// <configuration>
    ///    <configSections>
    ///       <section name="queueit.security" type="QueueIT.Security.Configuration.SettingsSection, QueueIT.Security"/>
    ///    </configSections>
    ///    <queueit.security>
    ///       <repositorySettings>
    ///           <setting name="IdleExpiration" value="00:03:00" />
    ///           <setting name="ExtendValidity" value="true" />
    ///       </repositorySettings>
    ///    </queueit.security>
    /// </configuration>    
    /// ]]>
    /// </code>    
    /// </example>
    public class SessionValidateResultRepository : ValidateResultRepositoryBase
    {
        static SessionValidateResultRepository()
        {
            LoadConfiguration();
        }

        private static void LoadConfiguration()
        {
            ValidateResultRepositoryBase.LoadBaseConfiguration();
        }

        /// <summary>
        /// Configures the SessionValidateResultRepository. This method will override any previous calls and configuration in config files.
        /// </summary>
        /// <param name="idleExpiration">The amount of time the user can stay on the website before sent to the queue if the queue is in Idle mode. The time will not be extended each time validation is performed.</param>
        /// <param name="extendValidity">If false, the time will not be extended each time validation is performed.</param>
        public static void Configure(
            TimeSpan idleExpiration = default(TimeSpan),
            bool extendValidity = true)
        {
            ValidateResultRepositoryBase.ConfigureBase(idleExpiration, extendValidity);
        }

        internal static void Clear()
        {
            ValidateResultRepositoryBase.ClearBase();
        }

        public override IValidateResult GetValidationResult(IQueue queue)
        {
            var key = GenerateKey(queue.CustomerId, queue.EventId);
            SessionStateModel model = HttpContext.Current.Session[key] as SessionStateModel;

            if (model == null)
                return null;
            if (model.Expiration.HasValue && model.Expiration < DateTime.UtcNow)
                return null;

            return new AcceptedConfirmedResult(
                queue, 
                new Md5KnownUser(
                    model.QueueId,
                    model.PlaceInQueue,
                    model.TimeStamp,
                    queue.CustomerId,
                    queue.EventId,
                    model.RedirectType,
                    model.OriginalUri), 
                false);
        }

        public override void SetValidationResult(IQueue queue, IValidateResult validationResult, DateTime? expirationTime = null)
        {
            AcceptedConfirmedResult acceptedResult = validationResult as AcceptedConfirmedResult;

            if (acceptedResult != null)
            {
                var key = GenerateKey(queue.CustomerId, queue.EventId);
                SessionStateModel model = new SessionStateModel()
                {
                    QueueId = acceptedResult.KnownUser.QueueId,
                    OriginalUri = acceptedResult.KnownUser.OriginalUrl,
                    PlaceInQueue = acceptedResult.KnownUser.PlaceInQueue,
                    TimeStamp = acceptedResult.KnownUser.TimeStamp,
                    RedirectType = acceptedResult.KnownUser.RedirectType,
                };

                if (expirationTime != null)
                    model.Expiration = expirationTime;
                else if (acceptedResult.KnownUser.RedirectType == RedirectType.Idle)
                    model.Expiration = DateTime.UtcNow.Add(IdleExpiration);
                else if (!ExtendValidity)
                    model.Expiration = DateTime.UtcNow.AddMinutes(HttpContext.Current.Session.Timeout);

                HttpContext.Current.Session[key] = model;
            }
        }

        public override void Cancel(IQueue queue, IValidateResult validationResult)
        {
            var key = GenerateKey(queue.CustomerId, queue.EventId);
            HttpContext.Current.Session.Remove(key);
        }
    }
}
