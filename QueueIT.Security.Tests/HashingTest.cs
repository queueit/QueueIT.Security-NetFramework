using QueueIT.Security;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Security.Cryptography;
using System.Text;
using System.Web;
namespace QueueIT.Security.Tests
{


    /// <summary>
    ///This is a test class for HashingTest and is intended
    ///to contain all HashingTest Unit Tests
    ///</summary>
    [TestClass()]
    public class HashingTest
    {


        private TestContext testContextInstance;

        /// <summary>
        ///Gets or sets the test context which provides
        ///information about and functionality for the current test run.
        ///</summary>
        public TestContext TestContext
        {
            get
            {
                return testContextInstance;
            }
            set
            {
                testContextInstance = value;
            }
        }

        #region Additional test attributes
        // 
        //You can use the following additional attributes as you write your tests:
        //
        //Use ClassInitialize to run code before running the first test in the class
        //[ClassInitialize()]
        //public static void MyClassInitialize(TestContext testContext)
        //{
        //}
        //
        //Use ClassCleanup to run code after all tests in a class have run
        //[ClassCleanup()]
        //public static void MyClassCleanup()
        //{
        //}
        //
        //Use TestInitialize to run code before running each test
        //[TestInitialize()]
        //public void MyTestInitialize()
        //{
        //}
        //
        //Use TestCleanup to run code after each test has run
        //[TestCleanup()]
        //public void MyTestCleanup()
        //{
        //}
        //
        #endregion


        /// <summary>
        ///A test for VerifySimpleHash
        ///</summary>
        [TestMethod()]
        public void VerifySimpleHashTest()
        {
            string SharedSecreteEventKey = "zaqxswcdevfrbgtnhymjukiloZAQCDEFRBGTNHYMJUKILOPlkjhgfdsapoiuytrewqmnbvcx";

            Assert.AreEqual(-2, Hashing.VerifySimpleHash("", "", "", 5141));
            Assert.AreEqual(-2, Hashing.VerifySimpleHash(string.Empty, "", "", 5141));
            Assert.AreEqual(-2, Hashing.VerifySimpleHash("", string.Empty, "", 5141));
            Assert.AreEqual(-2, Hashing.VerifySimpleHash("87a33946-8a4d-480c-bbe1-2311fa0779d6", string.Empty, "", 5141));
            Assert.AreEqual(-2, Hashing.VerifySimpleHash(string.Empty, "ce45e046-5fca-40f9-8161-4b5de023ab84", "", 5141));

            long placeInqueue = 7810;
            string QueueID = "87a33946-8a4d-480c-bbe1-2311fa0779d6"; //Fra query string
            string PlaceInQueue = "5770d948-0b09-4c17-b785-9611ef0ae03c"; //Fra query string 
            long ParsedHashValue = Hashing.GenerateSimpleHash(QueueID, PlaceInQueue, SharedSecreteEventKey); //Fra query string
            Assert.AreEqual(placeInqueue, Hashing.VerifySimpleHash(QueueID, PlaceInQueue, SharedSecreteEventKey, ParsedHashValue));


            placeInqueue = 20;
            QueueID = "b35ea550-08b3-4293-806b-ded839fcd013"; //Fra query string
            PlaceInQueue = "e1c06cd0-0007-4d21-90bc-20217f0b76ee"; //Fra query string 
            ParsedHashValue = Hashing.GenerateSimpleHash(QueueID, PlaceInQueue, SharedSecreteEventKey); //Fra query string
            Assert.AreEqual(placeInqueue, Hashing.VerifySimpleHash(QueueID, PlaceInQueue, SharedSecreteEventKey, ParsedHashValue));


            placeInqueue = 1;
            QueueID = "67bb1269-63a0-47cb-82b4-760712809ce2"; //Fra query string
            PlaceInQueue = "05a00770-150f-4993-a082-fb0da9028b51"; //Fra query string 
            ParsedHashValue = Hashing.GenerateSimpleHash(QueueID, PlaceInQueue, SharedSecreteEventKey); //Fra query string
            Assert.AreEqual(placeInqueue, Hashing.VerifySimpleHash(QueueID, PlaceInQueue, SharedSecreteEventKey, ParsedHashValue));


            placeInqueue = 4212870;
            QueueID = "5475dc74-8f02-408e-aae1-62e582c7764b"; //Fra query string
            PlaceInQueue = "b852fe78-0d10-4254-823c-f8749c401153"; //Fra query string 
            ParsedHashValue = Hashing.GenerateSimpleHash(QueueID, PlaceInQueue, SharedSecreteEventKey); //Fra query string
            Assert.AreEqual(placeInqueue, Hashing.VerifySimpleHash(QueueID, PlaceInQueue, SharedSecreteEventKey, ParsedHashValue));



            //Test tampered PlaceInQueue 
            placeInqueue = 4212870;
            QueueID = "5475dc74-8f02-408e-aae1-62e582c7764b"; //Fra query string
            PlaceInQueue = "b852fe78-0d10-4254-823c-f8749c101153"; //changed one char from org. b852fe78-0d10-4254-823c-f8749c401153
            Assert.AreEqual(-1, Hashing.VerifySimpleHash(QueueID, PlaceInQueue, SharedSecreteEventKey, ParsedHashValue));


            placeInqueue = 1;
            for (int i = 0; i < 1000; i++)
            {
                //simulate Queue-it web site generating the hash value
                Guid QID = Guid.NewGuid();
                placeInqueue += 1;
                string encryptPlaceInQueue = Hashing.EncryptPlaceInQueue(placeInqueue);
                ParsedHashValue = Hashing.GenerateSimpleHash(QID.ToString(), encryptPlaceInQueue, SharedSecreteEventKey); //Fra query string
                //verify simple hash 
                Assert.AreEqual(placeInqueue, Hashing.VerifySimpleHash(QID.ToString(), encryptPlaceInQueue, SharedSecreteEventKey, ParsedHashValue));
            }


        }

        /// <summary>
        ///A test for GenerateRandomSecretKey
        ///</summary>
        [TestMethod()]
        public void GenerateRandomSecretKeyTest()
        {
            string actual = Hashing.GenerateRandomSecretKey(72);
            actual = Hashing.GenerateRandomSecretKey(72);
            actual = Hashing.GenerateRandomSecretKey(72);
            actual = Hashing.GenerateRandomSecretKey(72);
            actual = Hashing.GenerateRandomSecretKey(72);
            actual = Hashing.GenerateRandomSecretKey(72);
        }

        /// <summary>
        ///A test for VerifyHMACSHA256Hash
        ///</summary>
        [TestMethod()]
        public void VerifyHMACSHA256HashTest()
        {
            string Password = Hashing.GenerateRandomSecretKey(120);

            string PageRequestUrlOriginalString = string.Empty; // TODO: Initialize to an appropriate value
            string QueueId = string.Empty; // TODO: Initialize to an appropriate value
            string PlaceInQueueEncryptString = string.Empty; // TODO: Initialize to an appropriate value
            string ParsedHash = string.Empty; // TODO: Initialize to an appropriate value
            long actual;
            actual = Hashing.VerifyHMACSHA256Hash(PageRequestUrlOriginalString, QueueId, PlaceInQueueEncryptString, ParsedHash, Password);

            long placeInQueue = 1;
            for (int i = 0; i < 10; i++)
            {
                //simulate Queue-it web site generating the url to redirect to incl. hash vale
                Guid QID = Guid.NewGuid();
                placeInQueue += 1;
                string redirectUrl = @"https://www.billetlugen.dk/koeb/billetter/?token=AvO4IsSbEcHP8Glb%2btU2W eACt4xPb9CZEiO0GTMsGnvl6tpbui8vwM%2bQdMysp26FCMUXGliNzSHz8qYfEPcK6V8wdX/ 9zoCjrjjMy2twZlnG1liE6ebGSbi74magiUntyyTyHscjMQGiKqhXRnyDtQr/GBhSHFBz7b7 diUQ0cwg%3d";
                string encryptPlaceInQueue = Hashing.EncryptPlaceInQueue(placeInQueue);
                redirectUrl = redirectUrl + "&q=" + QID.ToString() + "&p=" + encryptPlaceInQueue;

                string UrlEncodedHashValue = Hashing.GenerateHMACSHA256Hash(Password, QID.ToString(), redirectUrl);
                string redirectUrlHash = redirectUrl + "&h=" + UrlEncodedHashValue;
                actual = Hashing.VerifyHMACSHA256Hash(redirectUrlHash, QID.ToString(), encryptPlaceInQueue, UrlEncodedHashValue, Password);

                Assert.AreEqual(placeInQueue, actual, "Hash failed");

                //simulate a users manipulating the placeInQueue string
                if (encryptPlaceInQueue.EndsWith("2"))
                    encryptPlaceInQueue = encryptPlaceInQueue.Substring(0, encryptPlaceInQueue.Length - 1) + "1";
                else
                    encryptPlaceInQueue = encryptPlaceInQueue.Substring(0, encryptPlaceInQueue.Length - 1) + "2";

                redirectUrlHash = redirectUrl + "&h=" + UrlEncodedHashValue;
                actual = Hashing.VerifyHMACSHA256Hash(redirectUrl, QID.ToString(), encryptPlaceInQueue, UrlEncodedHashValue, Password);
                Assert.AreEqual(actual, -1, "Fake url allowed");
                // Console.Write(sw.ElapsedMilliseconds);

            }
        }

        [TestMethod()]
        public void PlaceInQueueTest()
        {
            long expectedPlaceInQueue = 0;
            string encryptedPlaceInQueue = Hashing.EncryptPlaceInQueue(expectedPlaceInQueue);
            long decryptedPlaceInQueue = Hashing.DecryptPlaceInQueue(encryptedPlaceInQueue);
            Assert.AreEqual(expectedPlaceInQueue, decryptedPlaceInQueue);

            expectedPlaceInQueue = 46;
            encryptedPlaceInQueue = Hashing.EncryptPlaceInQueue(expectedPlaceInQueue);
            decryptedPlaceInQueue = Hashing.DecryptPlaceInQueue(encryptedPlaceInQueue);
            Assert.AreEqual(expectedPlaceInQueue, decryptedPlaceInQueue);

            expectedPlaceInQueue = 9999999;
            encryptedPlaceInQueue = Hashing.EncryptPlaceInQueue(expectedPlaceInQueue);
            decryptedPlaceInQueue = Hashing.DecryptPlaceInQueue(encryptedPlaceInQueue);
            Assert.AreEqual(expectedPlaceInQueue, decryptedPlaceInQueue);
        }

        [TestMethod()]
        [ExpectedException(typeof(ArgumentException))]
        public void PlaceInQueue_invalidTest()
        {
            string encryptedPlaceInQueue = "";
            long decryptedPlaceInQueue = Hashing.DecryptPlaceInQueue(encryptedPlaceInQueue);
        }

        /// <summary>
        ///A test for GetTimestamp
        ///</summary>
        [TestMethod()]
        public void GetTimestampTest()
        {

            DateTime d = DateTime.UtcNow;
            long actual;
            actual = Hashing.GetTimestamp();
            Assert.AreNotEqual(0, actual);


            DateTime w = d.AddYears(200);
            TimeSpan q = w.Subtract(new DateTime(1970, 1, 1, 0, 0, 0, 0));
            long qq = (long)q.TotalSeconds;



            DateTime actualDateTime = Hashing.TimestampToDateTime(actual);

            Assert.AreEqual(d.Hour, actualDateTime.Hour);
            Assert.AreEqual(d.Minute, actualDateTime.Minute);
            Assert.AreEqual(d.Second, actualDateTime.Second);
        }

        /// <summary>
        ///A test for GenerateSimpleHashWithTimestamp
        ///</summary>
        [TestMethod()]
        public void GenerateSimpleHashWithTimestampTest()
        {
            string SharedSecreteEventKey = "zaqxswcdevfrbgtnhymjukiloZAQCDEFRBGTNHYMJUKILOPlkjhgfdsapoiuytrewqmnbvcx";

            Assert.AreEqual(-2, Hashing.VerifySimpleHashWithTimestamp("", "", 0, "", 5141));
            Assert.AreEqual(-2, Hashing.VerifySimpleHashWithTimestamp(string.Empty, "", 0, "", 5141));
            Assert.AreEqual(-2, Hashing.VerifySimpleHashWithTimestamp("", string.Empty, 0, "", 5141));
            Assert.AreEqual(-2, Hashing.VerifySimpleHashWithTimestamp("87a33946-8a4d-480c-bbe1-2311fa0779d6", string.Empty, 0, "", 5141));
            Assert.AreEqual(-2, Hashing.VerifySimpleHashWithTimestamp(string.Empty, "ce45e046-5fca-40f9-8161-4b5de023ab84", 0, "", 5141));
            //long Timestamp = Hashing.GetTimestamp();
            //Assert.AreEqual(-2, Hashing.VerifySimpleHashWithTimestamp("87a33946-8a4d-480c-bbe1-2311fa0779d6", "5770d948-0b09-4c17-b785-9611ef0ae03c", 0, "", 5141));

            long placeInqueue = 7810;
            string QueueID = "87a33946-8a4d-480c-bbe1-2311fa0779d6"; //Fra query string
            string PlaceInQueue = "5770d948-0b09-4c17-b785-9611ef0ae03c"; //Fra query string 
            long Timestamp = Hashing.GetTimestamp();
            long ParsedHashValue = Hashing.GenerateSimpleHashWithTimestamp(QueueID, PlaceInQueue, Timestamp, SharedSecreteEventKey); //Fra query string
            Assert.AreEqual(placeInqueue, Hashing.VerifySimpleHashWithTimestamp(QueueID, PlaceInQueue, Timestamp, SharedSecreteEventKey, ParsedHashValue));
            Assert.IsTrue(DateTime.UtcNow < Hashing.TimestampToDateTime(Timestamp).AddSeconds(90));

            placeInqueue = 20;
            QueueID = "b35ea550-08b3-4293-806b-ded839fcd013"; //Fra query string
            PlaceInQueue = "e1c06cd0-0007-4d21-90bc-20217f0b76ee"; //Fra query string 
            Timestamp = Hashing.GetTimestamp();
            ParsedHashValue = Hashing.GenerateSimpleHashWithTimestamp(QueueID, PlaceInQueue, Timestamp, SharedSecreteEventKey); //Fra query string
            Assert.AreEqual(placeInqueue, Hashing.VerifySimpleHashWithTimestamp(QueueID, PlaceInQueue, Timestamp, SharedSecreteEventKey, ParsedHashValue));
            Assert.IsTrue(DateTime.UtcNow < Hashing.TimestampToDateTime(Timestamp).AddSeconds(90));


            placeInqueue = 1;
            QueueID = "67bb1269-63a0-47cb-82b4-760712809ce2"; //Fra query string
            PlaceInQueue = "05a00770-150f-4993-a082-fb0da9028b51"; //Fra query string 
            ParsedHashValue = Hashing.GenerateSimpleHashWithTimestamp(QueueID, PlaceInQueue, Timestamp, SharedSecreteEventKey); //Fra query string
            Assert.AreEqual(placeInqueue, Hashing.VerifySimpleHashWithTimestamp(QueueID, PlaceInQueue, Timestamp, SharedSecreteEventKey, ParsedHashValue));
            Assert.IsTrue(DateTime.UtcNow < Hashing.TimestampToDateTime(Timestamp).AddSeconds(90));


            placeInqueue = 4212870;
            QueueID = "5475dc74-8f02-408e-aae1-62e582c7764b"; //Fra query string
            PlaceInQueue = "b852fe78-0d10-4254-823c-f8749c401153"; //Fra query string 
            Timestamp = Hashing.GetTimestamp();
            ParsedHashValue = Hashing.GenerateSimpleHashWithTimestamp(QueueID, PlaceInQueue, Timestamp, SharedSecreteEventKey); //Fra query string
            Assert.AreEqual(placeInqueue, Hashing.VerifySimpleHashWithTimestamp(QueueID, PlaceInQueue, Timestamp, SharedSecreteEventKey, ParsedHashValue));
            Assert.IsTrue(DateTime.UtcNow < Hashing.TimestampToDateTime(Timestamp).AddSeconds(90));

            //Test Timestamp timeout
            System.Threading.Thread.Sleep(2000);
            DateTime hashTime = Hashing.TimestampToDateTime(Timestamp);
            Assert.IsFalse(DateTime.UtcNow < hashTime.AddSeconds(1));


            //Test tampered PlaceInQueue 
            placeInqueue = 4212870;
            QueueID = "5475dc74-8f02-408e-aae1-62e582c7764b"; //Fra query string
            PlaceInQueue = "b852fe78-0d10-4254-823c-f8749c101153"; //changed one char from org. b852fe78-0d10-4254-823c-f8749c401153
            Timestamp = Hashing.GetTimestamp();
            Assert.AreEqual(-1, Hashing.VerifySimpleHashWithTimestamp(QueueID, PlaceInQueue, Timestamp, SharedSecreteEventKey, ParsedHashValue));
            Assert.IsTrue(DateTime.UtcNow < Hashing.TimestampToDateTime(Timestamp).AddSeconds(90));

            placeInqueue = 1;
            for (int i = 0; i < 1000; i++)
            {
                //simulate Queue-it web site generating the hash value
                Guid QID = Guid.NewGuid();
                placeInqueue += 1;
                string encryptPlaceInQueue = Hashing.EncryptPlaceInQueue(placeInqueue);
                Timestamp = Hashing.GetTimestamp();
                ParsedHashValue = Hashing.GenerateSimpleHashWithTimestamp(QID.ToString(), encryptPlaceInQueue, Timestamp, SharedSecreteEventKey); //Fra query string
                //verify simple hash 
                Assert.AreEqual(placeInqueue, Hashing.VerifySimpleHashWithTimestamp(QID.ToString(), encryptPlaceInQueue, Timestamp, SharedSecreteEventKey, ParsedHashValue));
                Assert.IsTrue(DateTime.UtcNow < Hashing.TimestampToDateTime(Timestamp).AddSeconds(90));
            }
        }

        [TestMethod()]
        public void GenerateMD5HashTest_sdf()
        {
            string SharedSecreteEventKey = "20afffb6-e53e-4cc0-bdad-002bc2229869bae279af-d75b-4f8e-9506-a3e0e1809f94";
            using (MD5 md5Hash = MD5.Create())
            {

                var x = "e287421a68125ec521c024628b47d478";
                string url = "http://gsusaload.ebiz.uapps.net/vp/FamilyManagement/Activities/ActivityLanding.aspx?pid=25&q=55f0c593-1011-4fc8-9573-dc41224d3d3a&p=a5209082-050f-44cc-94a9-8307d309f44d&ts=1389906944&c=girlscouts&e=demo01&rt=Queue&h=";
                string hash = Hashing.GenerateMD5Hash(url, SharedSecreteEventKey);

                Assert.AreEqual(x, hash);
            }
        }

        /// <summary>
                        ///A test for GenerateSimpleHashWithTimestamp
                        ///</summary>
                    [
                    TestMethod()]
        public void GenerateMD5HashTest()
        {
            string SharedSecreteEventKey = "zaqxswcdevfrbgtnhymjukiloZAQCDEFRBGTNHYMJUKILOPlkjhgfdsapoiuytrewqmnbvcx";
            using (MD5 md5Hash = MD5.Create())
            {


                long placeInqueue = 7810;
                string QueueID = "87a33946-8a4d-480c-bbe1-2311fa0779d6"; //Fra query string
                string PlaceInQueue = "5770d948-0b09-4c17-b785-9611ef0ae03c"; //Fra query string 
                long Timestamp = Hashing.GetTimestamp();
                string url = "http://q.queue-it.net/inqueue.aspx?c=mpro&e=hashingtest&q=" + QueueID +
                    "&p=" + PlaceInQueue + "&ts" + Timestamp + "&h=";
                string hash = Hashing.GenerateMD5Hash(url, SharedSecreteEventKey);
                url = url + hash;
                Assert.AreEqual(placeInqueue, Hashing.VerifyMD5Hash(md5Hash, url, SharedSecreteEventKey, PlaceInQueue));
                Assert.IsTrue(DateTime.UtcNow < Hashing.TimestampToDateTime(Timestamp).AddMinutes(3));

                placeInqueue = 20;
                QueueID = "b35ea550-08b3-4293-806b-ded839fcd013"; //Fra query string
                PlaceInQueue = "e1c06cd0-0007-4d21-90bc-20217f0b76ee"; //Fra query string 
                Timestamp = Hashing.GetTimestamp();
                url = "http://q.queue-it.net/inqueue.aspx?c=mpro&e=hashingtest&q=" + QueueID +
                    "&p=" + PlaceInQueue + "&ts" + Timestamp + "&h=";
                hash = Hashing.GenerateMD5Hash(url, SharedSecreteEventKey);
                url = url + hash;
                Assert.AreEqual(placeInqueue, Hashing.VerifyMD5Hash(md5Hash, url, SharedSecreteEventKey, PlaceInQueue));
                Assert.IsTrue(DateTime.UtcNow < Hashing.TimestampToDateTime(Timestamp).AddMinutes(3));


                placeInqueue = 1;
                QueueID = "67bb1269-63a0-47cb-82b4-760712809ce2"; //Fra query string
                PlaceInQueue = "05a00770-150f-4993-a082-fb0da9028b51"; //Fra query string 
                Timestamp = Hashing.GetTimestamp();
                url = "http://q.queue-it.net/inqueue.aspx?c=mpro&e=hashingtest&q=" + QueueID +
                    "&p=" + PlaceInQueue + "&ts" + Timestamp + "&h=";
                hash = Hashing.GenerateMD5Hash(url, SharedSecreteEventKey);
                url = url + hash;
                Assert.AreEqual(placeInqueue, Hashing.VerifyMD5Hash(md5Hash, url, SharedSecreteEventKey, PlaceInQueue));
                Assert.IsTrue(DateTime.UtcNow < Hashing.TimestampToDateTime(Timestamp).AddMinutes(3));

                placeInqueue = 4212870;
                QueueID = "5475dc74-8f02-408e-aae1-62e582c7764b"; //Fra query string
                PlaceInQueue = "b852fe78-0d10-4254-823c-f8749c401153"; //Fra query string 
                Timestamp = Hashing.GetTimestamp();
                url = "http://q.queue-it.net/inqueue.aspx?c=mpro&e=hashingtest&q=" + QueueID +
                    "&p=" + PlaceInQueue + "&ts" + Timestamp + "&h=";
                hash = Hashing.GenerateMD5Hash(url, SharedSecreteEventKey);
                url = url + hash;
                Assert.AreEqual(placeInqueue, Hashing.VerifyMD5Hash(md5Hash, url, SharedSecreteEventKey, PlaceInQueue));
                Assert.IsTrue(DateTime.UtcNow < Hashing.TimestampToDateTime(Timestamp).AddMinutes(3));


                placeInqueue = 4212870;
                QueueID = "5475dc74-8f02-408e-aae1-62e582c7764b"; //Fra query string
                PlaceInQueue = "b852fe78-0d10-4254-823c-f8749c401153"; //Fra query string 
                Timestamp = Hashing.GetTimestamp();
                url = "http://q.queue-it.net/inqueue.aspx?c=mpro&e=hashingtest&q=" + QueueID +
                        "&p=" + PlaceInQueue + "&ts" + Timestamp + "&h=";
                hash = Hashing.GenerateMD5Hash(url, SharedSecreteEventKey);
                url = url + hash;
                Assert.AreEqual(placeInqueue, Hashing.VerifyMD5Hash(md5Hash, url, SharedSecreteEventKey, PlaceInQueue));
                Assert.IsTrue(DateTime.UtcNow < Hashing.TimestampToDateTime(Timestamp).AddMinutes(3));

                //Test Timestamp timeout
                System.Threading.Thread.Sleep(2000);
                DateTime hashTime = Hashing.TimestampToDateTime(Timestamp);
                Assert.IsFalse(DateTime.UtcNow < hashTime.AddSeconds(1));


                //Test tampered PlaceInQueue 
                placeInqueue = 4212870;
                QueueID = "5475dc74-8f02-408e-aae1-62e582c7764b"; //Fra query string
                PlaceInQueue = "b852fe78-0d10-4254-823c-f8749c101153"; //changed one char from org. b852fe78-0d10-4254-823c-f8749c401153
                Timestamp = Hashing.GetTimestamp();
                url = "http://q.queue-it.net/inqueue.aspx?c=mpro&e=hashingtest&q=" + QueueID +
                        "&p=" + PlaceInQueue + "&ts" + Timestamp + "&h=";
                url = url + hash;
                Assert.AreEqual(-1, Hashing.VerifyMD5Hash(md5Hash, url, SharedSecreteEventKey, PlaceInQueue));
                Assert.IsTrue(DateTime.UtcNow < Hashing.TimestampToDateTime(Timestamp).AddMinutes(3));

            }
        }

        /// <summary>
        ///A test for ExtendSharedEventKey
        ///</summary>
        [TestMethod()]
        [DeploymentItem("QueueIT.Security.dll")]
        public void ExtendSharedEventKeyTest()
        {
            string SharedSecreteEventKey = "zaqxswcdevfrbgtnhymjukiloZAQCDEFRBGTNHYMJUKILOPlkjhgfdsapoiuytrewqmnbvcx";
            string expected = "zaqxswcdevfrbgtnhymjukiloZAQCDEFRBGTNHYMJUKILOPlkjhgfdsapoiuytrewqmnbvcxzaqxswcdevfrbgtnhymjukiloZAQ";
            string actual;
            actual = Hashing.ExtendSharedEventKey(SharedSecreteEventKey, expected.Length);
            Assert.AreEqual(expected, actual);
            actual = Hashing.ExtendSharedEventKey(SharedSecreteEventKey, 20);
            Assert.AreEqual(SharedSecreteEventKey, actual);
        }


        [TestMethod()]
        [ExpectedException(typeof(ArgumentException))]
        public void InputValidationTest()
        {
            Hashing.GenerateSimpleHash(null, "jhjhh", "jkj");
            Hashing.GenerateSimpleHash("757657", null, "jkj");
            Hashing.GenerateSimpleHash("757657", "dhfkhd", null);
            Hashing.GenerateSimpleHash(string.Empty, "jhjhh", "jkj");
            Hashing.GenerateSimpleHash("757657", string.Empty, "jkj");
            Hashing.GenerateSimpleHash("757657", "dhfkhd", string.Empty);

            Hashing.GenerateHMACSHA256Hash(null, null, null);
            Hashing.GenerateSimpleHashWithTimestamp(null, null, 1, null);
        }

        [TestMethod()]
        public void VerifySimpleHash_DisabledQueue_Test()
        {
            string Password = "9d919dfb-00e2-4919-8695-469f5ebc91f7930edb9f-2339-4deb-864e-5f26269691b6";

            // Url taken from service
            // http://www.google.com?q=00000000-0000-0000-0000-000000000000&p=86b9b819-9791-45fc-b96d-7d9c1090fef7&h=277950

            long actual = Hashing.VerifySimpleHash("00000000-0000-0000-0000-000000000000", "86b9b819-9791-45fc-b96d-7d9c1090fef7", Password, 277950);

            Assert.AreEqual(9999999, actual);
        }

        [TestMethod()]
        public void VerifyHMACSHA256Hash_DisabledQueue_Test()
        {
            string Password = "9d919dfb-00e2-4919-8695-469f5ebc91f7930edb9f-2339-4deb-864e-5f26269691b6";

            // Url taken from service
            string redirectUrl = HttpUtility.UrlDecode("http://www.google.com?q=00000000-0000-0000-0000-000000000000&p=87493fc9-9b96-4507-8932-db9bd29f127a&h=%255d%251c)%253f%253c%253f3%253fJ%253f%2518%253fN%253f%253f0v%253f%253f%253fhw%253f%253f%253fJ%2518%253f%253f%253f%253f.");

            long actual = Hashing.VerifyHMACSHA256Hash(redirectUrl, "00000000-0000-0000-0000-000000000000", "87493fc9-9b96-4507-8932-db9bd29f127a", "%255d%251c)%253f%253c%253f3%253fJ%253f%2518%253fN%253f%253f0v%253f%253f%253fhw%253f%253f%253fJ%2518%253f%253f%253f%253f.", Password);

            Assert.AreEqual(9999999, actual);
        }
        [TestMethod()]
        public void VerifySimpleWithTimestampHash_DisabledQueue_Test()
        {
            string Password = "9d919dfb-00e2-4919-8695-469f5ebc91f7930edb9f-2339-4deb-864e-5f26269691b6";

            // Url taken from service
            // http://www.google.com/?q=00000000-0000-0000-0000-000000000000&p=ffd93309-9b96-4654-a9a3-da9547920863&ts=1332333410&h=294215

            long actual = Hashing.VerifySimpleHashWithTimestamp("00000000-0000-0000-0000-000000000000", "ffd93309-9b96-4654-a9a3-da9547920863", 1332333410, Password, 294215);

            Assert.AreEqual(9999999, actual);
        }

    }
}
