using System;
using System.Security.Cryptography;
using System.Text;
using System.Web;
using QueueIT.Security.Configuration;

namespace QueueIT.Security
{
    /// <summary>
    /// A repository to store user session state which stores the validation result in a http cookie. 
    /// </summary>
    public class CookieValidateResultRepository : ValidateResultRepositoryBase
    {
        private static string CookieDomain;

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
            }
        }

        /// <summary>
        /// Configures the CookieValidateResultRepository. This method will override any previous calls and configuration in config files.
        /// </summary>
        /// <param name="cookieDomain">The domain name of the cookie scope</param>
        public static void Configure(string cookieDomain = null)
        {
            if (cookieDomain != null)
                CookieDomain = cookieDomain;
        }

        internal static void Clear()
        {
            CookieDomain = null;
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
                var key = GenerateKey(queue.CustomerId, queue.EventId);
                HttpCookie validationCookie = new HttpCookie(key);

                string queueId = acceptedResult.KnownUser.QueueId.ToString();
                string originalUrl = acceptedResult.KnownUser.OriginalUrl.AbsoluteUri;
                int placeInQueue = acceptedResult.KnownUser.PlaceInQueue.HasValue ? acceptedResult.KnownUser.PlaceInQueue.Value : 0;
                string redirectType = acceptedResult.KnownUser.RedirectType.ToString();
                string timeStamp = Hashing.GetTimestamp(acceptedResult.KnownUser.TimeStamp).ToString();

                string hash = GenerateHash(queueId, originalUrl, placeInQueue.ToString(), redirectType, timeStamp);

                validationCookie.Values["QueueId"] = queueId;
                validationCookie.Values["OriginalUrl"] = originalUrl;
                validationCookie.Values["PlaceInQueue"] = Hashing.EncryptPlaceInQueue(placeInQueue);
                validationCookie.Values["RedirectType"] = redirectType;
                validationCookie.Values["TimeStamp"] = timeStamp;
                validationCookie.Values["Hash"] = hash;

                validationCookie.HttpOnly = true;
                validationCookie.Domain = CookieDomain;

                HttpContext.Current.Response.Cookies.Add(validationCookie);
            }
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
