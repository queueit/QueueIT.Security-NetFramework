using System;
using System.Security.Cryptography;
using System.Text;
using System.Web;
using QueueIT.Security.Configuration;

namespace QueueIT.Security
{
    /// <summary>
    /// A repository to store user session state which stores the validation result in a http cookie. This is the default.
    /// </summary>
    /// <remarks>
    /// This will by default store a cookie with 20 minutes expiration.
    /// </remarks>
    /// <example>
    /// <code language="cs">
    /// CookieValidateResultRepository.Configure(".ticketania.com", TimeSpan.FromMinutes(20));
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
            SettingsSection settings = SettingsSection.GetSection();
            if (settings != null && settings.RepositorySettings != null)
            {
                CookieDomain = GetValue("CookieDomain", settings.RepositorySettings);
                string cookieExpirationString = GetValue("CookieExpiration", settings.RepositorySettings);

                TimeSpan cookieExpiration;
                if (TimeSpan.TryParse(cookieExpirationString, out cookieExpiration))
                    CookieExpiration = cookieExpiration;
            }
        }

        /// <summary>
        /// Configures the CookieValidateResultRepository. This method will override any previous calls and configuration in config files.
        /// </summary>
        /// <param name="cookieDomain">The domain name of the cookie scope</param>
        /// <param name="cookieExpiration">The amount of time the user can stay on the website before sent to the queue. The time will be extended each time validation is performed.</param>
        public static void Configure(string cookieDomain = null, TimeSpan cookieExpiration = default(TimeSpan))
        {
            if (cookieDomain != null)
                CookieDomain = cookieDomain;
            if (cookieExpiration != default(TimeSpan))
                CookieExpiration = cookieExpiration;
        }

        internal static void Clear()
        {
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
                string originalUrl = validationCookie.Values["OriginalUrl"];
                int placeInQueue = (int)Hashing.DecryptPlaceInQueue(validationCookie.Values["PlaceInQueue"]);
                string redirectType = validationCookie.Values["RedirectType"];
                string timeStamp = validationCookie.Values["TimeStamp"];
                string actualHash = validationCookie.Values["Hash"];

                string expectedHash = GenerateHash(queueId, originalUrl, placeInQueue.ToString(), redirectType, timeStamp);

                if (actualHash != expectedHash)
                    return null;

                SetCookie(queue, queueId, originalUrl, placeInQueue, redirectType, timeStamp, actualHash);

                return new AcceptedConfirmedResult(
                    queue,
                    new Md5KnownUser(
                        new Guid(queueId),
                        placeInQueue,
                        Hashing.TimestampToDateTime(long.Parse(timeStamp)),
                        queue.CustomerId,
                        queue.EventId,
                        (RedirectType)Enum.Parse(typeof(RedirectType), redirectType),
                        new Uri(originalUrl)), false);
            }
            catch (ArgumentException)
            {
                return null;
            }
        }

        public override void SetValidationResult(IQueue queue, IValidateResult validationResult)
        {

            AcceptedConfirmedResult acceptedResult = validationResult as AcceptedConfirmedResult;

            if (acceptedResult != null)
            {
                string queueId = acceptedResult.KnownUser.QueueId.ToString();
                string originalUrl = acceptedResult.KnownUser.OriginalUrl.AbsoluteUri;
                int placeInQueue = acceptedResult.KnownUser.PlaceInQueue.HasValue ? acceptedResult.KnownUser.PlaceInQueue.Value : 0;
                string redirectType = acceptedResult.KnownUser.RedirectType.ToString();
                string timeStamp = Hashing.GetTimestamp(acceptedResult.KnownUser.TimeStamp).ToString();

                string hash = GenerateHash(queueId, originalUrl, placeInQueue.ToString(), redirectType, timeStamp);

                SetCookie(queue, queueId, originalUrl, placeInQueue, redirectType, timeStamp, hash);
            }
        }

        private static void SetCookie(
            IQueue queue, 
            string queueId, 
            string originalUrl, 
            int placeInQueue, 
            string redirectType,
            string timeStamp, 
            string hash)
        {
            var key = GenerateKey(queue.CustomerId, queue.EventId);
            HttpCookie validationCookie = new HttpCookie(key);
            validationCookie.Values["QueueId"] = queueId;
            validationCookie.Values["OriginalUrl"] = originalUrl;
            validationCookie.Values["PlaceInQueue"] = Hashing.EncryptPlaceInQueue(placeInQueue);
            validationCookie.Values["RedirectType"] = redirectType;
            validationCookie.Values["TimeStamp"] = timeStamp;
            validationCookie.Values["Hash"] = hash;

            validationCookie.HttpOnly = true;
            validationCookie.Domain = CookieDomain;
            validationCookie.Expires = DateTime.UtcNow.Add(CookieExpiration);

            HttpContext.Current.Response.Cookies.Add(validationCookie);
        }

        private string GenerateHash(
            string queueId, 
            string originalUrl, 
            string placeInQueue, 
            string redirectType, 
            string timestamp)
        {
            using (SHA256 sha2 = SHA256.Create())
            {
                string valueToHash = string.Concat(queueId, originalUrl, placeInQueue, redirectType, timestamp, KnownUserFactory.SecretKey);
                byte[] hash = sha2.ComputeHash(Encoding.UTF8.GetBytes(valueToHash));

                return BitConverter.ToString(hash);
            }
        }
    }
}
