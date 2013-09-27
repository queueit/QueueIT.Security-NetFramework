using System;
using System.Linq;
using System.Security.Cryptography;
using QueueIT.Security.Configuration;

namespace QueueIT.Security
{
    /// <summary>
    /// Provides factory methods to verify Known User requests
    /// </summary>
    /// <remarks>
    /// View members for additional information and examples
    /// </remarks>
    public static class KnownUserFactory
    {
        private static string _defaultSecretKey;
        private static string _defaultQuerystringPrefix;
        private static Func<IKnownUserUrlProvider> _defaultUrlProviderFactory;

        static KnownUserFactory()
        {
            _defaultUrlProviderFactory = () => new DefaultKnownUserUrlProvider();

            LoadConfiguration();
        }

        private static void LoadConfiguration()
        {
            SettingsSection settings = SettingsSection.GetSection();
            if (settings != null)
            {
                _defaultSecretKey = settings.SecretKey;
                _defaultQuerystringPrefix = settings.QueryStringPrefix;
            }
        }

        /// <summary>
        /// Configures the KnownUserFactory. This method will override any previous calls and configuration in config files.
        /// </summary>
        /// <param name="secretKey">The secret key as configured on the queue</param>
        /// <param name="urlProviderFactory">
        /// An optional way of providing the original hashed URL (in case of URL rewrite or similar)
        /// </param>
        /// <param name="querystringPrefix">
        /// An optional querystring prefix as configured on the Queue-it account. 
        /// This can be used if there are name collisions with the queuestring parameters appended by Queue-it 
        /// </param>
        public static void Configure(
            string secretKey = null,
            Func<IKnownUserUrlProvider> urlProviderFactory = null,
            string querystringPrefix = null)
        {
            if (secretKey != null)
                _defaultSecretKey = secretKey;
            if (urlProviderFactory != null)
                _defaultUrlProviderFactory = urlProviderFactory;
            if (querystringPrefix != null)
                _defaultQuerystringPrefix = querystringPrefix;
        }

        /// <summary>
        /// Verifies a MD5 Known User request
        /// </summary>
        /// <param name="secretKey">
        /// The secret key as configured on the queue
        /// </param>
        /// <param name="urlProvider">
        /// An optional way of providing the original hashed URL (in case of URL rewrite or similar)
        /// </param>
        /// <param name="querystringPrefix">The request to verify
        /// An optional querystring prefix as configured on the Queue-it account. 
        /// This can be used if there are name collisions with the queuestring parameters appended by Queue-it 
        /// </param>
        /// <returns>IKnownUser reprecentation of the request</returns>
        /// <exception cref="System.ArgumentNullException">
        /// The Secret Key cannot be null. Invoke KnownUserFactory.Configure or add configuration in config file.
        /// </exception>
        /// <exception cref="QueueIT.Security.InvalidKnownUserUrlException">
        /// The Known User request does not contaion the required parameters
        /// </exception>
        /// <exception cref="QueueIT.Security.InvalidKnownUserHashException">
        /// The hash of the request is invalid
        /// </exception>
        /// <example>
        /// <code language="cs">
        /// try
        /// {
        ///     IKnownUser knownUser = KnownUserFactory.VerifyMd5Hash();
        ///
        ///     if (knownUser == null)
        ///         throw new UnverifiedKnownUserException();
        ///
        ///     if (knownUser.TimeStamp  &lt; DateTime.UtcNow.Subtract(TimeSpan.FromMinutes(3)))
        ///         throw new UnverifiedKnownUserException();
        ///
        ///     PersistModel model = new PersistModel(
        ///         knownUser.QueueId,
        ///         knownUser.PlaceInQueue,
        ///         knownUser.TimeStamp);
        ///
        ///     model.Persist();
        /// }
        /// catch (KnownUserException ex)
        /// {
        ///     Response.Redirect("Error.aspx?queuename=link");
        /// }
        /// </code>
        /// </example>
        /// <example>
        /// PHP example:
        /// <code language="none">
        /// <![CDATA[
        /// <?php
        /// 	require_once('../QueueIT.Security PHP/KnownUserFactory.php');
        /// 	
        /// 	use QueueIT\Security\KnownUserFactory, QueueIT\Security\KnownUserException;
        /// 	
        /// 	try
        /// 	{
        /// 		$knownUser = KnownUserFactory::verifyMd5Hash();
        /// 	
        /// 		if ($knownUser == null)
        /// 			header('Location: link.php');
        /// 				
        /// 		if ($knownUser->getTimeStamp()->getTimestamp() < (time() - 180))
        /// 			header('Location: link.php');
        /// 	}
        /// 	catch (KnownUserException $ex)
        /// 	{
        /// 		header('Location: error.php');
        /// 	}
        /// ?>
        /// ]]>
        /// </code>
        /// </example>
        /// <example>
        /// Java EE example:
        /// <code language="none">
        /// <![CDATA[
        ///     try
        ///     {
        ///         IKnownUser knownUser = KnownUserFactory.verifyMd5Hash();
        /// 
        ///         if (knownUser == null) {
        ///             response.sendRedirect("link.jsp");
        ///             return;
        ///         }
        /// 
        ///         if (knownUser.getTimeStamp().getTime()  < ((new Date()).getTime() - 180 * 1000)) {
        ///             response.sendRedirect("link.jsp");
        ///             return;
        ///         }
        ///     }
        ///     catch (KnownUserException ex)
        ///     {
        ///         response.sendRedirect("error.jsp");
        ///         return;
        ///     }
        /// ]]>
        /// </code>
        /// </example>
        public static IKnownUser VerifyMd5Hash(
            string secretKey = null, 
            IKnownUserUrlProvider urlProvider = null, 
            string querystringPrefix = null)
        {
            if (string.IsNullOrEmpty(secretKey))
                secretKey = _defaultSecretKey;

            if (string.IsNullOrEmpty(querystringPrefix))
                querystringPrefix = _defaultQuerystringPrefix;

            if (urlProvider == null && _defaultUrlProviderFactory != null)
                urlProvider = _defaultUrlProviderFactory.Invoke();

            if (string.IsNullOrEmpty(secretKey))
                throw new ArgumentNullException(
                    "secretKey", 
                    "The Secret Key cannot be null. Invoke KnownUserFactory. Configure or add configuration in config file.");

            Uri originalUrl = urlProvider.GetOriginalUrl(querystringPrefix);

            try
            {
                Uri url = urlProvider.GetUrl();
                Guid? queueId = ParseQueueId(urlProvider.GetQueueId(querystringPrefix));
                string placeInQueueObfuscated = urlProvider.GetPlaceInQueue(querystringPrefix);
                int? placeInQueue = null; 
                if (!string.IsNullOrEmpty(placeInQueueObfuscated))
                {
                    try
                    {
                        placeInQueue = (int)Hashing.DecryptPlaceInQueue(placeInQueueObfuscated);
                    }
                    catch (Exception)
                    {
                        throw new InvalidKnownUserUrlException();
                    }
                }

                DateTime? timeStamp = ParseTimeStamp(urlProvider.GetTimeStamp(querystringPrefix));
                string customerId = urlProvider.GetCustomerId(querystringPrefix);
                string eventId = urlProvider.GetEventId(querystringPrefix);
                RedirectType redirectType = ParseRedirectType(urlProvider.GetRedirectType(querystringPrefix));

                if (!queueId.HasValue && !placeInQueue.HasValue && !timeStamp.HasValue)
                    return null;

                if (!queueId.HasValue || !placeInQueue.HasValue || !timeStamp.HasValue)
                    throw new InvalidKnownUserUrlException();

                string expectedHash = GetExpectedHash(url);

                ValidateHash(url, secretKey, expectedHash);

                return new Md5KnownUser(queueId.Value, placeInQueue.Value, timeStamp.Value, customerId, eventId, redirectType, originalUrl);
            }
            catch (InvalidKnownUserHashException ex)
            {
                ex.OriginalUrl = originalUrl;
                throw;
            }
            catch(InvalidKnownUserUrlException ex)
            {
                ex.OriginalUrl = originalUrl;
                throw;
            }
        }

        private static RedirectType ParseRedirectType(string redirectType)
        {
            if (string.IsNullOrEmpty(redirectType))
                return RedirectType.Unknown;

            if (Enum.GetNames(typeof(RedirectType)).FirstOrDefault(value => redirectType.ToLower() == value.ToLower()) == null)
                return RedirectType.Unknown;

            return (RedirectType) Enum.Parse(typeof (RedirectType), redirectType, true);
        }

        private static DateTime? ParseTimeStamp(string timeStamp)
        {
            int timestampSeconds;

            if (string.IsNullOrEmpty(timeStamp))
                return null;

            if (!int.TryParse(timeStamp, out timestampSeconds))
                throw new InvalidKnownUserUrlException();

            return Hashing.TimestampToDateTime(timestampSeconds);
        }

        private static Guid? ParseQueueId(string getQueueId)
        {
            if (string.IsNullOrEmpty(getQueueId))
                return null;

            try
            {
                return new Guid(getQueueId);
            }
            catch (Exception)
            {
                throw new InvalidKnownUserUrlException();
            }
        }

        internal static void Reset(bool loadConfiguration)
        {
            _defaultQuerystringPrefix = null;
            _defaultSecretKey = null;
            _defaultUrlProviderFactory = () => new DefaultKnownUserUrlProvider();

            if (loadConfiguration)
                LoadConfiguration();
        }

        private static void ValidateHash(Uri url, string secretKey, string expectedHash)
        {
            string hashString = url.AbsoluteUri.Substring(0, url.AbsoluteUri.Length - 32) + secretKey; //Remove hash value and add SharedEventKey

            using (MD5 md5 = MD5.Create())
            {
                string actualHash = Hashing.GetMd5Hash(md5, hashString);

                if (StringComparer.OrdinalIgnoreCase.Compare(expectedHash, actualHash) != 0)
                    throw new InvalidKnownUserHashException();
            }
        }

        private static string GetExpectedHash(Uri url)
        {
            string fullUrl = url.AbsoluteUri;

            if (fullUrl == null || fullUrl.Length < 32)
                throw new InvalidKnownUserHashException();

            return fullUrl.Substring(fullUrl.Length - 32);
        }
    }
}
