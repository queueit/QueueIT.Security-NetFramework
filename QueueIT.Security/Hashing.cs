using System;
using System.Text;
using System.Security.Cryptography;
using System.Web;


namespace QueueIT.Security
{
    /// <summary>
    /// Provides helper methods to hash Known User requests
    /// </summary>
    public static class Hashing
    {
        #region HMACSHA256Hash

        [Obsolete("Please use MD5 hashing for future implementations")]
        public static string GenerateHMACSHA256Hash(string Password, string QueueId, string UrlToHash)
        {
            ValidateInput(QueueId, "QueueId");
            ValidateInput(Password, "Password");
            ValidateInput(UrlToHash, "UrlToHash");

            // Create a random key using a random number generator. This would be the
            //  secret key shared by sender and receiver.
            byte[] secretkey = new Byte[64];
            Rfc2898DeriveBytes deriveBytes = new Rfc2898DeriveBytes(Password, Encoding.UTF8.GetBytes(QueueId));
            secretkey = deriveBytes.GetBytes(64);

            // Initialize the keyed hash object.
            HMACSHA256 myhmacsha256 = new HMACSHA256(secretkey);
            byte[] byteStringToHash = Encoding.UTF8.GetBytes(UrlToHash);
            byte[] hashValue = myhmacsha256.ComputeHash(byteStringToHash);
            myhmacsha256.Clear();

            string hashStr = Encoding.ASCII.GetString(hashValue);
            string UrlEncodeHashString = HttpUtility.UrlEncode(hashStr);
            //not all special characters like = and / are encoded on first UrlEncoding
            //UrlEncoding once more fixes the problem......
            UrlEncodeHashString = HttpUtility.UrlEncode(UrlEncodeHashString);

            return UrlEncodeHashString;
        }

        [Obsolete("Please use MD5 hashing for future implementations")]
        public static long VerifyHMACSHA256Hash(string PageRequestUrlOriginalString, string QueueId, string PlaceInQueueEncryptString, string ParsedHash, string Password)
        {
            if (string.IsNullOrEmpty(PlaceInQueueEncryptString)) return -2;

            int hashResult = VerifyHMACSHA256Hash(PageRequestUrlOriginalString, QueueId, ParsedHash, Password);

            if (hashResult != 0)
                return hashResult;

            return DecryptPlaceInQueue(PlaceInQueueEncryptString);
        }

        private static int VerifyHMACSHA256Hash(string PageRequestUrlOriginalString, string QueueId, string ParsedHash, string Password)
        {
            try
            {
                if (!string.IsNullOrEmpty(QueueId) & !string.IsNullOrEmpty(ParsedHash))
                {
                    //Verify that the queueIdStr can be transformed to a GUID
                    Guid a = new Guid(QueueId);
                    //Verify that the placeInQueueEncryptString can be transformed to a GUID

                    string stringToHash = PageRequestUrlOriginalString.Substring(0, PageRequestUrlOriginalString.LastIndexOf("&"));
                    string CalculatedHash = GenerateHMACSHA256Hash(Password, QueueId, stringToHash);

                    if (!ParsedHash.Equals(CalculatedHash))
                        return -1; //the hash parsed by query string did not match hash of url (the requested url is not valid)

                    return 0;

                }

                return -2; //required parms not provided
            }
            catch (Exception)
            {
                return -2;
            }
        }

        #endregion

        #region Encrypt/Decrypt QueueNumber

        /// <summary>
        /// Obfuscate a queue number
        /// </summary>
        /// <param name="placeInQueue">The queue number</param>
        /// <returns>The obfuscated queue number</returns>
        public static string EncryptPlaceInQueue(long placeInQueue)
        {
            Guid a = Guid.NewGuid();
            string g = a.ToString();
            string p = placeInQueue.ToString("0000000");
            char[] c = g.ToCharArray();
            c[9] = p[6];
            c[26] = p[5];
            c[7] = p[4];
            c[20] = p[3];
            c[11] = p[2];
            c[3] = p[1];
            c[30] = p[0];
            string placeInQueueEncryp = new string(c);
            return placeInQueueEncryp;
        }

        /// <summary>
        /// Unobfuscate a queue number
        /// </summary>
        /// <param name="encryptedPlaceInQueue">The obfuscated queue number</param>
        /// <returns>The queue number</returns>
        public static long DecryptPlaceInQueue(string encryptedPlaceInQueue)
        {
            ValidateInput(encryptedPlaceInQueue, "encryptedPlaceInQueue");

            string e = encryptedPlaceInQueue;
            string p = e.Substring(30, 1) + e.Substring(3, 1) + e.Substring(11, 1) + e.Substring(20, 1) + e.Substring(7, 1) + e.Substring(26, 1) + e.Substring(9, 1);
            return int.Parse(p);
        }

        [Obsolete("Use DecryptPlaceInQueue")]
        public static long DecryptPlaceInQueue6Digits(string encryptedPlaceInQueue)
        {
            ValidateInput(encryptedPlaceInQueue, "encryptedPlaceInQueue");

            string e = encryptedPlaceInQueue;
            string p = e.Substring(30, 1) + e.Substring(3, 1) + e.Substring(11, 1) + e.Substring(20, 1) + e.Substring(7, 1) + e.Substring(26, 1);
            return int.Parse(p);
        }

        #endregion

        #region SimpleHash

        [Obsolete("Please use MD5 hashing for future implementations")]
        public static long GenerateSimpleHash(string QueueId, string PlaceInQueueEncryptString, string SharedEventKey)
        {
            ValidateInput(QueueId, "QueueId");
            ValidateInput(PlaceInQueueEncryptString, "PlaceInQueueEncryptString");
            ValidateInput(SharedEventKey, "SharedEventKey");

            string StringToHash = QueueId + PlaceInQueueEncryptString;
            SharedEventKey = ExtendSharedEventKey(SharedEventKey, StringToHash.Length); //make sure that the SharedEventKey is minimum as long as the string to hash
            return _GenerateSimpleHas(StringToHash, SharedEventKey);
        }


        [Obsolete("Please use MD5 hashing for future implementations")]
        public static long VerifySimpleHash(string QueueId, string PlaceInQueueEncryptString, string SharedEventKey, long ParsedCheckValue)
        {
            try
            {
                string StringToHash = QueueId + PlaceInQueueEncryptString;
                SharedEventKey = ExtendSharedEventKey(SharedEventKey, StringToHash.Length); //make sure that the SharedEventKey is minimum as long as the string to hash
                return _VerifySimpleHash(StringToHash, PlaceInQueueEncryptString, SharedEventKey, ParsedCheckValue);
            }
            catch (Exception)
            {
                return -2;
            }
        }

        #endregion

        #region SimpleHash with time limit

        /// <summary>
        /// Generates a UNIX timestpam from UTC Now
        /// </summary>
        /// <returns>Seconds since 1970</returns>
        public static long GetTimestamp(DateTime? dateStamp = null)
        {
            if (!dateStamp.HasValue)
                dateStamp = DateTime.UtcNow;

            TimeSpan TimeSpanSince1970 = dateStamp.Value.Subtract(new DateTime(1970, 1, 1, 0, 0, 0, 0));
            double SecondsSince1970 = TimeSpanSince1970.TotalSeconds;

            return (long)SecondsSince1970;
        }

        /// <summary>
        /// Converts a UNIX timestamp to a DateTime object
        /// </summary>
        /// <param name="timestamp">Seconds since 1970</param>
        /// <returns>The DateTime object</returns>
        public static DateTime TimestampToDateTime(long timestamp)
        {
            DateTime Date1970 = new DateTime(1970, 1, 1, 0, 0, 0, 0);
            return Date1970.AddSeconds(timestamp);
        }

        [Obsolete("Please use MD5 hashing for future implementations")]
        public static long GenerateSimpleHashWithTimestamp(string QueueId, string PlaceInQueueEncryptString, long Timestamp, string SharedEventKey)
        {
            ValidateInput(QueueId, "QueueId");
            ValidateInput(PlaceInQueueEncryptString, "PlaceInQueueEncryptString");
            ValidateInput(SharedEventKey, "SharedEventKey");

            string StringToHash = QueueId + PlaceInQueueEncryptString + Timestamp.ToString();
            SharedEventKey = ExtendSharedEventKey(SharedEventKey, StringToHash.Length); //make sure that the SharedEventKey is minimum as long as the string to hash
            return _GenerateSimpleHas(StringToHash, SharedEventKey);
        }

        [Obsolete("Please use MD5 hashing for future implementations")]
        public static long VerifySimpleHashWithTimestamp(string QueueId, string PlaceInQueueEncryptString, long Timestamp, string SharedEventKey, long ParsedCheckValue)
        {
            try
            {
                string StringToHash = QueueId + PlaceInQueueEncryptString + Timestamp.ToString();
                SharedEventKey = ExtendSharedEventKey(SharedEventKey, StringToHash.Length); //make sure that the SharedEventKey is minimum as long as the string to hash
                return _VerifySimpleHash(StringToHash, PlaceInQueueEncryptString, SharedEventKey, ParsedCheckValue);
            }
            catch (Exception)
            {
                return -2;
            }
        }

        #endregion

        #region MD5Hash

        /// <summary>
        /// Genereates an MD5 hash of a url usin a secret key
        /// </summary>
        /// <param name="Url">The url to generate hash from</param>
        /// <param name="SharedEventKey">The secret key</param>
        /// <returns>The generated MD5 hash as HEX</returns>
        public static string GenerateMD5Hash(string Url, string SharedEventKey)
        {
            using (MD5 md5Hash = MD5.Create())
            {
                string hash = GetMd5Hash(md5Hash, Url + SharedEventKey);
                return hash;
            }
        }

        /// <summary>
        /// Verifies that the URL has not been tampered with using a secret key
        /// </summary>
        /// <param name="md5Hash">An MD5 instance used genereate hashes</param>
        /// <param name="url">The URL to verify</param>
        /// <param name="SharedEventKey">The secret key</param>
        /// <param name="PlaceInQueueEncryptString">The encrypted queue number (the p-parameter)</param>
        /// <returns>Returns -1 if the URL has been tampered with - otherwhise the queue number of the request. The queue number may be 9999999 if the queue number is unknown</returns>
        public static long VerifyMD5Hash(MD5 md5Hash, string url, string SharedEventKey, string PlaceInQueueEncryptString)
        {
            try
            {
                //input must not be empty
                if (!string.IsNullOrEmpty(url) & !string.IsNullOrEmpty(SharedEventKey) & !string.IsNullOrEmpty(PlaceInQueueEncryptString))
                {
                    long placeInQueue = -1;
                    string parsedHashValue = url.Substring(url.Length - 32); //The hash value is the last 32 chars of the URL
                    string input = url.Substring(0, url.Length - 32) + SharedEventKey; //Remove hash value and add SharedEventKey

                    // Hash the input.
                    string hashOfInput = GetMd5Hash(md5Hash, input);

                    // Create a StringComparer an compare the hashes.
                    StringComparer comparer = StringComparer.OrdinalIgnoreCase;

                    if (0 == comparer.Compare(hashOfInput, parsedHashValue))
                    {
                        placeInQueue = DecryptPlaceInQueue(PlaceInQueueEncryptString);
                    }
                    else
                    {
                        placeInQueue = -1;
                    }

                    return placeInQueue;
                }
                else
                    return -2; //Input values not valid
            }
            catch (Exception)
            {
                return -2;
            }
        }

        internal static string GetMd5Hash(MD5 md5Hash, string input)
        {
            // Convert the input string to a byte array and compute the hash.
            byte[] data = md5Hash.ComputeHash(Encoding.UTF8.GetBytes(input));

            // Create a new Stringbuilder to collect the bytes
            // and create a string.
            StringBuilder sBuilder = new StringBuilder();

            // Loop through each byte of the hashed data 
            // and format each one as a hexadecimal string.
            for (int i = 0; i < data.Length; i++)
            {
                sBuilder.Append(data[i].ToString("x2"));
            }

            // Return the hexadecimal string.
            return sBuilder.ToString();
        }

        #endregion


        private static long _GenerateSimpleHas(string StringToHash, string SharedEventKey)
        {
            //input must not be empty
            SharedEventKey = ExtendSharedEventKey(SharedEventKey, StringToHash.Length);
            char[] c = StringToHash.ToCharArray();
            char[] d = SharedEventKey.ToCharArray();
            long calculatedCheckValue = 0;
            for (int i = 0; i < StringToHash.Length; i++)
            {
                calculatedCheckValue = calculatedCheckValue + (c[i] * d[i]); //multiply the Unicode 16-bit character (0-65535)
            }
            return calculatedCheckValue;
        }

        private static long _VerifySimpleHash(string StringToHash, string PlaceInQueueEncryptString, string SharedEventKey, long ParsedCheckValue)
        {
            try
            {
                //input must not be empty
                if (!string.IsNullOrEmpty(StringToHash) & !string.IsNullOrEmpty(SharedEventKey))
                {
                    //input should be of same length
                    if (StringToHash.Length <= SharedEventKey.Length)
                    {
                        char[] c = StringToHash.ToCharArray();
                        char[] d = SharedEventKey.ToCharArray();
                        long calculatedCheckValue = 0;
                        for (int i = 0; i < StringToHash.Length; i++)
                        {
                            calculatedCheckValue = calculatedCheckValue + (c[i] * d[i]); //multiply the Unicode 16-bit character (0-65535)
                        }
                        long placeInQueue = DecryptPlaceInQueue(PlaceInQueueEncryptString);
                        if (calculatedCheckValue != ParsedCheckValue)
                            placeInQueue = -1; //the hash parsed by query string did not match hash of url (the requested url is not valid)
                        return placeInQueue;
                    }
                    else
                        return -2; //required parms not provided (StringToHash.Length > SharedEventKey.Length)
                }
                else
                    return -2; //required parms not provided
            }
            catch (Exception)
            {
                return -2;
            }
        }

        private static void ValidateInput(string Input, string Name)
        {
            if (string.IsNullOrEmpty(Input))
                throw new ArgumentException(Name + " is empty or null");
        }

        internal static string ExtendSharedEventKey(string SharedEventKey, int Length)
        {
            if (SharedEventKey.Length > 0) //we can only extend a SharedEventKey if the length is > 0
            {
                while (SharedEventKey.Length < Length)
                {
                    for (int i = 0; i < SharedEventKey.Length; i++)
                    {
                        if (SharedEventKey.Length < Length)
                            SharedEventKey += SharedEventKey.Substring(i, 1);
                    }
                }
            } return SharedEventKey;
        }

        /// <summary>
        /// Generates a ransom secret key
        /// </summary>
        /// <param name="Length">The lenght of the key</param>
        /// <returns>The key</returns>
        public static string GenerateRandomSecretKey(int Length)
        {
            byte[] str = new byte[Length * 2];
            Random r = new Random(Guid.NewGuid().GetHashCode());
            for (int i = 0; i < Length * 2; i += 2)
            {
                int chr = r.Next(0x21, 0x7E); // max allowed Unicode value = 0xD7FF
                str[i + 1] = (byte)((chr & 0xFF00) >> 8);
                str[i] = (byte)(chr & 0xFF);
            }

            return Encoding.Unicode.GetString(str);
        }


    }
}
