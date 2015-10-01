using System;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Web;
using QueueIT.Security.Configuration;

namespace QueueIT.Security
{
    /// <summary>
    /// A repository to store state which stores the validation result in a http cookie. This is the default.
    /// </summary>
    /// <remarks>
    /// This will by default store a cookie with 20 minutes expiration and the cookie will be renewed for every page request. 
    /// A cookie with a default expiration of 3 minutes will be set if the queue is in Idle or Disabled mode. 
    /// After the expiration a new request will be made to put the user in the queue<br /><br />
    /// NOTE: Users will be looped back to the queue if their browser does not support cookies. 
    /// It is highly recommended that your application confirms that there is cookie support before sending the user to the queue. 
    /// If there is no cookie support we recommend displaying an error message to the user asking them to enable cookies.
    /// </remarks>
    /// <example>
    /// <code language="cs">
    /// CookieValidateResultRepository.Configure(".ticketania.com", TimeSpan.FromMinutes(20), TimeSpan.FromMinutes(3), TimeSpan.FromMinutes(3));
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
    ///           <setting name="CookieDomain" value=".ticketania.com" />
    ///           <setting name="CookieExpiration" value="00:20:00" />
    ///           <setting name="IdleExpiration" value="00:03:00" />
    ///           <setting name="DisabledExpiration" value="00:03:00" />
    ///           <setting name="ExtendValidity" value="true" />
    ///       </repositorySettings>
    ///    </queueit.security>
    /// </configuration>    
    /// ]]>
    /// </code>
    /// </example>

    public class CookieValidateResultRepository : ValidateResultRepositoryBase
    {
        private static string CookieDomain;
        private static TimeSpan CookieExpiration = TimeSpan.FromMinutes(20);

        static CookieValidateResultRepository()
        {
            LoadConfiguration();
        }

        private static void LoadConfiguration()
        {
            ValidateResultRepositoryBase.LoadBaseConfiguration();

            SettingsSection settings = SettingsSection.GetSection();
            if (settings != null && settings.RepositorySettings != null)
            {
                CookieDomain = GetValue("CookieDomain", settings.RepositorySettings);
                
                SetTimespanFromRepositorySettings(
                    settings.RepositorySettings, "CookieExpiration", (value) => CookieExpiration = value);
            }
        }

        /// <summary>
        /// Configures the CookieValidateResultRepository. This method will override any previous calls and configuration in config files.
        /// </summary>
        /// <param name="cookieDomain">The domain name of the cookie scope</param>
        /// <param name="cookieExpiration">The amount of time the user can stay on the website before sent to the queue. The time will be extended each time validation is performed.</param>
        /// <param name="idleExpiration">The amount of time the user can stay on the website before sent to the queue if the queue is in Idle mode. The time will not be extended each time validation is performed.</param>
        /// <param name="extendValidity">If false, the time will not be extended each time validation is performed.</param>
        public static void Configure(
            string cookieDomain = null, 
            TimeSpan cookieExpiration = default(TimeSpan),
            TimeSpan idleExpiration = default(TimeSpan),
            bool extendValidity = true)
        {
            ValidateResultRepositoryBase.ConfigureBase(idleExpiration, extendValidity);

            if (cookieDomain != null)
                CookieDomain = cookieDomain;
            if (cookieExpiration != default(TimeSpan))
                CookieExpiration = cookieExpiration;
        }

        internal static void Clear()
        {
            ValidateResultRepositoryBase.ClearBase();

            CookieDomain = null;
            CookieExpiration = TimeSpan.FromMinutes(20);
        }

        public override IValidateResult GetValidationResult(IQueue queue)
        {
            try
            {
                var key = GenerateKey(queue.CustomerId, queue.EventId);

                HttpCookie validationCookie = HttpContext.Current.Request.Cookies.Get(key);
                if (validationCookie == null)
                    return null;

                string queueId = validationCookie.Values["QueueId"];
                string originalUrl = HttpUtility.UrlDecode(validationCookie.Values["OriginalUrl"]);
                int placeInQueue = (int)Hashing.DecryptPlaceInQueue(validationCookie.Values["PlaceInQueue"]);
                RedirectType redirectType = (RedirectType)Enum.Parse(typeof(RedirectType), validationCookie.Values["RedirectType"]);
                string timeStamp = validationCookie.Values["TimeStamp"];
                string actualHash = validationCookie.Values["Hash"];
                string expires = validationCookie.Values["Expires"];

                DateTime expirationTime = DateTime.MinValue;
                if (DateTime.TryParse(expires, out expirationTime))
                    expirationTime = expirationTime.ToUniversalTime();

                if (expirationTime < DateTime.UtcNow)
                    return null;

                string expectedHash = GenerateHash(
                    queueId, originalUrl, placeInQueue.ToString(), redirectType, timeStamp, expirationTime);

                if (actualHash != expectedHash)
                    return null;

                AcceptedConfirmedResult result = new AcceptedConfirmedResult(
                    queue,
                    new Md5KnownUser(
                        new Guid(queueId),
                        placeInQueue,
                        Hashing.TimestampToDateTime(long.Parse(timeStamp)),
                        queue.CustomerId,
                        queue.EventId,
                        redirectType,
                        originalUrl), 
                    false);

                if (ExtendValidity && result.KnownUser.RedirectType != RedirectType.Idle)
                {
                    DateTime newExpirationTime = DateTime.UtcNow.Add(CookieExpiration);
                    string newHash = GenerateHash(
                        queueId, originalUrl, placeInQueue.ToString(), redirectType, timeStamp, newExpirationTime);

                    SetCookie(
                        queue, 
                        queueId, 
                        originalUrl, 
                        placeInQueue, 
                        redirectType, 
                        timeStamp,
                        newHash,
                        newExpirationTime);
                }

                return result;
            }
            catch (ArgumentException)
            {
                return null;
            }
        }

        public override void SetValidationResult(IQueue queue, IValidateResult validationResult, DateTime? expirationTime = null)
        {
            AcceptedConfirmedResult acceptedResult = validationResult as AcceptedConfirmedResult;

            if (acceptedResult != null)
            {
                string queueId = acceptedResult.KnownUser.QueueId.ToString();
                string originalUrl = acceptedResult.KnownUser.OriginalUrl;
                int placeInQueue = acceptedResult.KnownUser.PlaceInQueue.HasValue ? acceptedResult.KnownUser.PlaceInQueue.Value : 0;
                RedirectType redirectType = acceptedResult.KnownUser.RedirectType;
                string timeStamp = Hashing.GetTimestamp(acceptedResult.KnownUser.TimeStamp).ToString();

                if (!expirationTime.HasValue)
                    expirationTime = DateTime.UtcNow.Add(redirectType == RedirectType.Idle ? IdleExpiration : CookieExpiration);

                string hash = GenerateHash(queueId, originalUrl, placeInQueue.ToString(), redirectType, timeStamp, expirationTime.Value);

                SetCookie(queue, queueId, originalUrl, placeInQueue, redirectType, timeStamp, hash, expirationTime.Value);
            }
        }

        public override void Cancel(IQueue queue, IValidateResult validationResult)
        {
            SetValidationResult(queue, validationResult, DateTime.UtcNow.AddDays(-1));
        }

        private static void SetCookie(
            IQueue queue, 
            string queueId, 
            string originalUrl, 
            int placeInQueue, 
            RedirectType redirectType,
            string timeStamp, 
            string hash,
            DateTime expirationTime)
        {
            var key = GenerateKey(queue.CustomerId, queue.EventId);
            HttpCookie validationCookie = new HttpCookie(key);
            validationCookie.Values["QueueId"] = queueId;
            validationCookie.Values["OriginalUrl"] = HttpUtility.UrlEncode(originalUrl);
            validationCookie.Values["PlaceInQueue"] = Hashing.EncryptPlaceInQueue(placeInQueue);
            validationCookie.Values["RedirectType"] = redirectType.ToString();
            validationCookie.Values["TimeStamp"] = timeStamp;
            validationCookie.Values["Hash"] = hash;

            validationCookie.HttpOnly = true;
            validationCookie.Domain = CookieDomain;
            validationCookie.Expires = expirationTime;
            validationCookie.Values["Expires"] = expirationTime.ToString("o");

            if (HttpContext.Current.Response.Cookies.AllKeys.Any(cookieKey => cookieKey == key))
                HttpContext.Current.Response.Cookies.Remove(key);
            HttpContext.Current.Response.Cookies.Add(validationCookie);
        }

        private string GenerateHash(
            string queueId, 
            string originalUrl, 
            string placeInQueue, 
            RedirectType redirectType, 
            string timestamp,
            DateTime expires)
        {
            using (SHA256 sha2 = SHA256.Create())
            {
                string valueToHash = string.Concat(
                    queueId, 
                    originalUrl, 
                    placeInQueue, 
                    redirectType.ToString(), 
                    timestamp, 
                    expires.ToString("o"),
                    KnownUserFactory.SecretKey);
                byte[] hash = sha2.ComputeHash(Encoding.UTF8.GetBytes(valueToHash));

                return BitConverter.ToString(hash);
            }
        }
    }
}
