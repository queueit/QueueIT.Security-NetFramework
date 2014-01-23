using System;
using System.Text;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace QueueIT.Security.Tests
{
    [TestClass]
    public class KnownUserFactoryTest
    {
        private const string SharedSecreteEventKey =
            "zaqxswcdevfrbgtnhymjukiloZAQCDEFRBGTNHYMJUKILOPlkjhgfdsapoiuytrewqmnbvcx";

        [TestInitialize]
        public void TestInitialize()
        {
            KnownUserFactory.Reset(false);
        }

        [TestMethod]
        public void KnownUserFactory_VerifyMd5HashTest_Test()
        {
            RunVerifyMd5HashTest(false, SharedSecreteEventKey);
        }

        [TestMethod]
        public void KnownUserFactory_VerifyMd5HashTest_ConfigurationSection_Test()
        {
            KnownUserFactory.Reset(true);

            RunVerifyMd5HashTest(true, null, "prefix");
        }

        [TestMethod]
        public void KnownUserFactory_Configure_Test()
        {
            KnownUserFactory.Configure(SharedSecreteEventKey, querystringPrefix: "prefix");

            RunVerifyMd5HashTest(false, null, "prefix");
        }

        private static void RunVerifyMd5HashTest(
            bool configLoaded, 
            string sharedSecreteEventKey = null, 
            string prefix = null, 
            string redirectTypeString = null,
            RedirectType redirectType = RedirectType.Unknown)
        {
            //Arrange
            int expectedPlaceInqueue = 7810;
            Guid expectedQueueId = Guid.NewGuid();
            string placeInQueueEncrypted = Hashing.EncryptPlaceInQueue(expectedPlaceInqueue);
            long unixTimestamp = Hashing.GetTimestamp();
            DateTime expectedTimeStamp = Hashing.TimestampToDateTime(unixTimestamp);
            string expectedCustomerId = "somecust";
            string expectedEventId = "someevent";

            string urlNoHash = "http://q.queue-it.net/inqueue.aspx?" + prefix + "c=somecust&" + prefix + "e=someevent&" + prefix + "q=" + expectedQueueId +
                "&" + prefix + "p=" + placeInQueueEncrypted + "&" + prefix + "ts=" + unixTimestamp + "&" + prefix + "rt=" + redirectTypeString + "&" + prefix + "h=";
            Uri hashUri = new Uri(urlNoHash);

            string hash = Hashing.GenerateMD5Hash(hashUri.AbsoluteUri, SharedSecreteEventKey);
            string querystring = prefix + "c=somecust&" + prefix + "e=someevent&" + prefix + "q=" + expectedQueueId +
                "&" + prefix + "p=" + placeInQueueEncrypted + "&" + prefix + "ts=" + unixTimestamp + "&" + prefix + "rt=" + redirectTypeString + "&" + prefix + "h=" + hash;
            string url = urlNoHash + hash;

            HttpRequest httpRequest = new HttpRequest("inqueue.aspx", url, querystring);
            HttpContext.Current = new HttpContext(httpRequest, new HttpResponse(null));

            //Act
            IKnownUser knownUser = KnownUserFactory.VerifyMd5Hash(
                configLoaded ? sharedSecreteEventKey : SharedSecreteEventKey, 
                querystringPrefix: configLoaded ? null : prefix);

            //Assert  
            Assert.IsNotNull(knownUser);
            Assert.AreEqual(expectedQueueId, knownUser.QueueId);
            Assert.IsTrue(knownUser.PlaceInQueue.HasValue);
            Assert.AreEqual(expectedPlaceInqueue, knownUser.PlaceInQueue);
            Assert.AreEqual(expectedTimeStamp, knownUser.TimeStamp);
            Assert.AreEqual(expectedCustomerId, knownUser.CustomerId);
            Assert.AreEqual(redirectType, knownUser.RedirectType);
            Assert.AreEqual(expectedEventId, knownUser.EventId);
        }

        [TestMethod]
        public void KnownUserFactory_VerifyMd5HashTest_WithPrefix_Test()
        {
            RunVerifyMd5HashTest(false, SharedSecreteEventKey, "prefix");
        }

        [TestMethod]
        public void KnownUserFactory_VerifyMd5HashTest_InvalidRedirectType_Test()
        {
            RunVerifyMd5HashTest(false, SharedSecreteEventKey, null, "invalidtype", RedirectType.Unknown);
        }

        [TestMethod]
        public void KnownUserFactory_VerifyMd5HashTest_RedirectType_Test()
        {
            RunVerifyMd5HashTest(false, SharedSecreteEventKey, null, "queue", RedirectType.Queue);
        }

        [TestMethod]
        public void KnownUserFactory_VerifyMd5HashTest_BilletlugenUrl_Test()
        {
            //Arrange
            int expectedPlaceInqueue = 7810;
            Guid expectedQueueID = Guid.NewGuid();
            string placeInQueueEncrypted = Hashing.EncryptPlaceInQueue(expectedPlaceInqueue);
            long unixTimestamp = Hashing.GetTimestamp();
            DateTime expectedTimeStamp = Hashing.TimestampToDateTime(unixTimestamp);

            string urlNoHash = "http://www.billetlugen.dk/direkte/?token=ZBixHRJxbOeyWsfo3ynInq64Ngp10zvS5R2N0jaVJNijzuZpsJTfx4iwIkBpAK8q4bbgPpF2o5RRF4vlxn5OzgjBM%2ffiWNqZuvIjvyqQGbRekYeSkmd6TA%3d%3d&q=" + expectedQueueID +
                "&p=" + placeInQueueEncrypted + "&ts=" + unixTimestamp + "&h=";
            Uri hashUri = new Uri(urlNoHash);

            string hash = Hashing.GenerateMD5Hash(hashUri.AbsoluteUri, SharedSecreteEventKey);
            string querystring = "token=ZBixHRJxbOeyWsfo3ynInq64Ngp10zvS5R2N0jaVJNijzuZpsJTfx4iwIkBpAK8q4bbgPpF2o5RRF4vlxn5OzgjBM%2ffiWNqZuvIjvyqQGbRekYeSkmd6TA%3d%3d&q=" + expectedQueueID + "&p=" + placeInQueueEncrypted + "&ts=" + unixTimestamp + "&h=" + hash;
            string url = urlNoHash + hash;

            HttpRequest httpRequest = new HttpRequest(null, url, querystring);
            HttpContext.Current = new HttpContext(httpRequest, new HttpResponse(null));

            //Act
            IKnownUser knownUser = KnownUserFactory.VerifyMd5Hash(SharedSecreteEventKey);

            //Assert  
            Assert.AreEqual(expectedQueueID, knownUser.QueueId);
            Assert.IsTrue(knownUser.PlaceInQueue.HasValue);
            Assert.AreEqual(expectedPlaceInqueue, knownUser.PlaceInQueue);
            Assert.AreEqual(expectedTimeStamp, knownUser.TimeStamp);
        }

        [TestMethod]
        [ExpectedException(typeof(InvalidKnownUserUrlException))]
        public void KnownUserFactory_VerifyMd5HashTest_InvalidQueueId_Test()
        {
            //Arrange
            int expectedPlaceInqueue = 7810;
            string placeInQueueEncrypted = Hashing.EncryptPlaceInQueue(expectedPlaceInqueue);
            long unixTimestamp = Hashing.GetTimestamp();

            string urlNoHash = "http://q.queue-it.net/inqueue.aspx?c=mpro&e=hashingtest&q=InvalidQueueId&p=" 
                + placeInQueueEncrypted + "&ts=" + unixTimestamp + "&h=";
            Uri hashUri = new Uri(urlNoHash);

            string hash = Hashing.GenerateMD5Hash(hashUri.AbsoluteUri, SharedSecreteEventKey);
            string querystring = "c=mpro&e=hashingtest&q=InvalidQueueId&p=" + placeInQueueEncrypted + "&ts=" + unixTimestamp + "&h=" + hash;
            string url = urlNoHash + hash;

            HttpRequest httpRequest = new HttpRequest("inqueue.aspx", url, querystring);
            HttpContext.Current = new HttpContext(httpRequest, new HttpResponse(null));

            //Act
            KnownUserFactory.VerifyMd5Hash(SharedSecreteEventKey);
        }

        [TestMethod]
        [ExpectedException(typeof(InvalidKnownUserUrlException))]
        public void KnownUserFactory_VerifyMd5HashTest_InvalidPlaceInQueue_Test()
        {
            //Arrange
            Guid expectedQueueID = Guid.NewGuid();
            string placeInQueueEncrypted = "b89a605c-8f51-4769-a1ee-5e22c30fd754"; //invalid
            long unixTimestamp = Hashing.GetTimestamp();

            string urlNoHash = "http://q.queue-it.net/inqueue.aspx?c=mpro&e=hashingtest&q=" + expectedQueueID +
                "&p=" + placeInQueueEncrypted + "&ts=" + unixTimestamp + "&h=";
            Uri hashUri = new Uri(urlNoHash);

            string hash = Hashing.GenerateMD5Hash(hashUri.AbsoluteUri, SharedSecreteEventKey);
            string querystring = "c=mpro&e=hashingtest&q=" + expectedQueueID + "&p=" + placeInQueueEncrypted + "&ts=" + unixTimestamp + "&h=" + hash;
            string url = urlNoHash + hash;

            HttpRequest httpRequest = new HttpRequest("inqueue.aspx", url, querystring);
            HttpContext.Current = new HttpContext(httpRequest, new HttpResponse(null));

            //Act
            KnownUserFactory.VerifyMd5Hash(SharedSecreteEventKey);
        }

        [TestMethod]
        [ExpectedException(typeof(InvalidKnownUserUrlException))]
        public void KnownUserFactory_VerifyMd5HashTest_InvalidTimeStamp_Test()
        {
            //Arrange
            Guid expectedQueueID = Guid.NewGuid();
            int expectedPlaceInqueue = 7810;
            string placeInQueueEncrypted = Hashing.EncryptPlaceInQueue(expectedPlaceInqueue);

            string urlNoHash = "http://q.queue-it.net/inqueue.aspx?c=mpro&e=hashingtest&q=" + expectedQueueID +
                "&p=" + placeInQueueEncrypted + "&ts=invalid&h=";
            Uri hashUri = new Uri(urlNoHash);

            string hash = Hashing.GenerateMD5Hash(hashUri.AbsoluteUri, SharedSecreteEventKey);
            string querystring = "c=mpro&e=hashingtest&q=" + expectedQueueID + "&p=" + placeInQueueEncrypted + "&ts=invalid&h=" + hash;
            string url = urlNoHash + hash;

            HttpRequest httpRequest = new HttpRequest("inqueue.aspx", url, querystring);
            HttpContext.Current = new HttpContext(httpRequest, new HttpResponse(null));

            //Act
            KnownUserFactory.VerifyMd5Hash(SharedSecreteEventKey);
        }

        [TestMethod]
        [ExpectedException(typeof(InvalidKnownUserHashException))]
        public void KnownUserFactory_VerifyMd5HashTest_InvalidHash_Test()
        {
            //Arrange
            Guid expectedQueueID = Guid.NewGuid();
            int expectedPlaceInqueue = 7810;
            string placeInQueueEncrypted = Hashing.EncryptPlaceInQueue(expectedPlaceInqueue);
            long unixTimestamp = Hashing.GetTimestamp();

            string urlNoHash = "http://q.queue-it.net/inqueue.aspx?c=mpro&e=hashingtest&q=" + expectedQueueID +
                "&p=" + placeInQueueEncrypted + "&ts=" + unixTimestamp + "&h=";

            string hash = "f83ab33400a630043591196134a01c01"; //invalid
            string querystring = "c=mpro&e=hashingtest&q=" + expectedQueueID + "&p=" + placeInQueueEncrypted + "&ts=" + unixTimestamp + "&h=" + hash;
            string url = urlNoHash + hash;

            HttpRequest httpRequest = new HttpRequest("inqueue.aspx", url, querystring);
            HttpContext.Current = new HttpContext(httpRequest, new HttpResponse(null));

            //Act
            KnownUserFactory.VerifyMd5Hash(SharedSecreteEventKey);
        }

        [TestMethod]
        public void KnownUserFactory_VerifyMd5HashTest_KnownUserException_ValidatedUrl_Test()
        {
            //Arrange
            Guid expectedQueueID = Guid.NewGuid();
            int expectedPlaceInqueue = 7810;
            string placeInQueueEncrypted = Hashing.EncryptPlaceInQueue(expectedPlaceInqueue);
            long unixTimestamp = Hashing.GetTimestamp();

            string urlNoHash = "http://q.queue-it.net/inqueue.aspx?c=mpro&e=hashingtest&q=" + expectedQueueID +
                "&p=" + placeInQueueEncrypted + "&ts=" + unixTimestamp + "&h=";

            string hash = "f83ab33400a630043591196134a01c01"; //invalid
            string querystring = "c=mpro&e=hashingtest&q=" + expectedQueueID + "&p=" + placeInQueueEncrypted + "&ts=" + unixTimestamp + "&h=" + hash;
            string url = urlNoHash + hash;

            HttpRequest httpRequest = new HttpRequest("inqueue.aspx", url, querystring);
            HttpContext.Current = new HttpContext(httpRequest, new HttpResponse(null));

            try
            {
                //Act
                KnownUserFactory.VerifyMd5Hash(SharedSecreteEventKey);
            }
            catch (KnownUserException ex)
            {
                Assert.AreEqual(url, ex.ValidatedUrl.AbsoluteUri);
            }
        }

        [TestMethod]
        public void KnownUserFactory_VerifyMd5HashTest_KnownUserException_OriginalUrl_Test()
        {
            //Arrange
            Guid expectedQueueID = Guid.NewGuid();
            int expectedPlaceInqueue = 7810;
            string placeInQueueEncrypted = Hashing.EncryptPlaceInQueue(expectedPlaceInqueue);
            long unixTimestamp = Hashing.GetTimestamp();

            string expectedOriginalUrl = "http://q.queue-it.net/inqueue.aspx";
            string urlNoHash = expectedOriginalUrl + "?c=mpro&e=hashingtest&q=" + expectedQueueID +
                "&p=" + placeInQueueEncrypted + "&ts=" + unixTimestamp + "&h=";

            string hash = "f83ab33400a630043591196134a01c01"; //invalid
            string querystring = "c=mpro&e=hashingtest&q=" + expectedQueueID + "&p=" + placeInQueueEncrypted + "&ts=" + unixTimestamp + "&h=" + hash;
            string url = urlNoHash + hash;

            HttpRequest httpRequest = new HttpRequest("inqueue.aspx", url, querystring);
            HttpContext.Current = new HttpContext(httpRequest, new HttpResponse(null));

            try
            {
                //Act
                KnownUserFactory.VerifyMd5Hash(SharedSecreteEventKey);

                Assert.Fail("Must throw exception");
            }
            catch (KnownUserException ex)
            {
                Assert.AreEqual(expectedOriginalUrl, ex.OriginalUrl.AbsoluteUri);
            }
        }

        [TestMethod]
        public void KnownUserFactory_VerifyMd5Hash_EmptyQueueId_Test()
        {
            string sharedSecreteEventKey = "9d919dfb-00e2-4919-8695-469f5ebc91f7930edb9f-2339-4deb-864e-5f26269691b6";
            string url =
                "http://www.google.com/";
            string querystring =
                "q=00000000-0000-0000-0000-000000000000&p=ac498cf9-9b9d-4014-a9d5-6794af9bae43&ts=1346745696&h=8541c1937f5b7211a5008326e9d997dc";

            HttpRequest httpRequest = new HttpRequest("inqueue.aspx", url, querystring);
            HttpContext.Current = new HttpContext(httpRequest, new HttpResponse(null));

            IKnownUser knownUser = KnownUserFactory.VerifyMd5Hash(sharedSecreteEventKey);

            Assert.AreEqual(Guid.Empty, knownUser.QueueId);
            Assert.AreEqual(null, knownUser.PlaceInQueue);
        }

        [TestMethod]
        public void KnownUserFactory_VerifyMd5Hash_NoParameters_Test()
        {
            string sharedSecreteEventKey = "9d919dfb-00e2-4919-8695-469f5ebc91f7930edb9f-2339-4deb-864e-5f26269691b6";
            string url =
                "http://www.google.com/";
            string querystring =
                "x=sdf";

            HttpRequest httpRequest = new HttpRequest("inqueue.aspx", url, querystring);
            HttpContext.Current = new HttpContext(httpRequest, new HttpResponse(null));

            IKnownUser knownUser = KnownUserFactory.VerifyMd5Hash(sharedSecreteEventKey);

            Assert.IsNull(knownUser);
        }

        [TestMethod]
        [ExpectedException(typeof(InvalidKnownUserUrlException))]
        public void KnownUserFactory_VerifyMd5Hash_OnlyQParameter_Test()
        {
            string sharedSecreteEventKey = "9d919dfb-00e2-4919-8695-469f5ebc91f7930edb9f-2339-4deb-864e-5f26269691b6";
            string url =
                "http://www.google.com/";
            string querystring =
                "q=" + Guid.NewGuid().ToString();

            HttpRequest httpRequest = new HttpRequest("inqueue.aspx", url, querystring);
            HttpContext.Current = new HttpContext(httpRequest, new HttpResponse(null));

            KnownUserFactory.VerifyMd5Hash(sharedSecreteEventKey);
        }

        [TestMethod]
        [ExpectedException(typeof(InvalidKnownUserUrlException))]
        public void KnownUserFactory_VerifyMd5Hash_OnlyPParameter_Test()
        {
            string sharedSecreteEventKey = "9d919dfb-00e2-4919-8695-469f5ebc91f7930edb9f-2339-4deb-864e-5f26269691b6";
            string url =
                "http://www.google.com/";
            string querystring =
                "p=" + Guid.NewGuid().ToString();

            HttpRequest httpRequest = new HttpRequest("inqueue.aspx", url, querystring);
            HttpContext.Current = new HttpContext(httpRequest, new HttpResponse(null));

            KnownUserFactory.VerifyMd5Hash(sharedSecreteEventKey);
        }

        [TestMethod]
        [ExpectedException(typeof(InvalidKnownUserUrlException))]
        public void KnownUserFactory_VerifyMd5Hash_OnlyTSParameter_Test()
        {
            string sharedSecreteEventKey = "9d919dfb-00e2-4919-8695-469f5ebc91f7930edb9f-2339-4deb-864e-5f26269691b6";
            string url =
                "http://www.google.com/";
            string querystring =
                "ts=" + Hashing.GetTimestamp();

            HttpRequest httpRequest = new HttpRequest("inqueue.aspx", url, querystring);
            HttpContext.Current = new HttpContext(httpRequest, new HttpResponse(null));

            KnownUserFactory.VerifyMd5Hash(sharedSecreteEventKey);
        }

        [TestMethod]
        public void KnownUserFactory_OriginalUri_Test()
        {
            int expectedPlaceInqueue = 7810;
            Guid expectedQueueID = Guid.NewGuid();
            string placeInQueueEncrypted = Hashing.EncryptPlaceInQueue(expectedPlaceInqueue);
            long unixTimestamp = Hashing.GetTimestamp();
            string expectedCustomerId = "somecust";
            string expectedEventId = "someevent";
            Uri expectedOriginalUrl = new Uri("http://www.google.com/search.aspx?x=sdfsdf&x=we&y=ert&urlencoded=%22Aardvarks+lurk%2c+OK%3f%22");

            string urlNoHash = expectedOriginalUrl.OriginalString + "&q=" + expectedQueueID +
                "&p=" + placeInQueueEncrypted + "&ts=" + unixTimestamp + "&c=" + expectedCustomerId + "&e=" + expectedEventId + "&h=";
            Uri hashUri = new Uri(urlNoHash);

            string hash = Hashing.GenerateMD5Hash(hashUri.AbsoluteUri, SharedSecreteEventKey);
            string querystring = "x=sdfsdf&x=we&y=ert&urlencoded=%22Aardvarks+lurk%2c+OK%3f%22&q=" + expectedQueueID + "&p=" + placeInQueueEncrypted + "&ts=" + unixTimestamp + "&c=" + expectedCustomerId + "&e=" + expectedEventId + "&h=" + hash;
            string url = urlNoHash + hash;

            HttpRequest httpRequest = new HttpRequest("inqueue.aspx", url, querystring);
            HttpContext.Current = new HttpContext(httpRequest, new HttpResponse(null));

            //Act
            IKnownUser knownUser = KnownUserFactory.VerifyMd5Hash(SharedSecreteEventKey);
            
            Assert.AreEqual(expectedOriginalUrl.AbsoluteUri.ToString(), knownUser.OriginalUrl.AbsoluteUri.ToString());
        }

        [TestMethod]
        public void KnownUserFactory_OriginalUri_NoParameters_Test()
        {
            int expectedPlaceInqueue = 7810;
            Guid expectedQueueID = Guid.NewGuid();
            string placeInQueueEncrypted = Hashing.EncryptPlaceInQueue(expectedPlaceInqueue);
            long unixTimestamp = Hashing.GetTimestamp();
            string expectedCustomerId = "somecust";
            string expectedEventId = "someevent";
            Uri expectedOriginalUrl = new Uri("http://www.google.com/");

            string urlNoHash = expectedOriginalUrl.OriginalString + "?q=" + expectedQueueID +
                "&p=" + placeInQueueEncrypted + "&ts=" + unixTimestamp + "&c=" + expectedCustomerId + "&e=" + expectedEventId + "&h=";
            Uri hashUri = new Uri(urlNoHash);

            string hash = Hashing.GenerateMD5Hash(hashUri.AbsoluteUri, SharedSecreteEventKey);
            string querystring = "q=" + expectedQueueID + "&p=" + placeInQueueEncrypted + "&ts=" + unixTimestamp + "&c=" + expectedCustomerId + "&e=" + expectedEventId + "&h=" + hash;
            string url = urlNoHash + hash;

            HttpRequest httpRequest = new HttpRequest("inqueue.aspx", url, querystring);
            HttpContext.Current = new HttpContext(httpRequest, new HttpResponse(null));

            //Act
            IKnownUser knownUser = KnownUserFactory.VerifyMd5Hash(SharedSecreteEventKey);

            Assert.AreEqual(expectedOriginalUrl.AbsoluteUri.ToString(), knownUser.OriginalUrl.AbsoluteUri.ToString());
        }

        [TestMethod]
        public void KnownUserFactory_OriginalUrl_InvalidHash_Test()
        {
            //Arrange
            Guid expectedQueueID = Guid.NewGuid();
            int expectedPlaceInqueue = 7810;
            string placeInQueueEncrypted = Hashing.EncryptPlaceInQueue(expectedPlaceInqueue);
            long unixTimestamp = Hashing.GetTimestamp();

            string urlNoHash = "http://q.queue-it.net/inqueue.aspx?q=" + expectedQueueID +
                "&p=" + placeInQueueEncrypted + "&ts=" + unixTimestamp + "&h=";

            string hash = "f83ab33400a630043591196134a01c01"; //invalid
            string querystring = "q=" + expectedQueueID + "&p=" + placeInQueueEncrypted + "&ts=" + unixTimestamp + "&h=" + hash;
            string url = urlNoHash + hash;

            HttpRequest httpRequest = new HttpRequest("inqueue.aspx", url, querystring);
            HttpContext.Current = new HttpContext(httpRequest, new HttpResponse(null));

            //Act
            try
            {
                KnownUserFactory.VerifyMd5Hash(SharedSecreteEventKey);

                Assert.Fail();
            }
            catch (InvalidKnownUserHashException ex)
            {
                Assert.AreEqual("http://q.queue-it.net/inqueue.aspx", ex.OriginalUrl.AbsoluteUri.ToString());
            }
            catch(Exception)
            {
                Assert.Fail();
            }
        }

        [TestMethod]
        public void KnownUserFactory_OriginalUrl_InvalidUrl_Test()
        {
            //Arrange
            string url = "http://q.queue-it.net/inqueue.aspx?q=yyyy&p=xxx&ts=345345&h=ttt";
            string querystring = "q=yyyy&p=xxx&ts=345345&h=ttt";
            HttpRequest httpRequest = new HttpRequest("inqueue.aspx", url, querystring);
            HttpContext.Current = new HttpContext(httpRequest, new HttpResponse(null));

            //Act
            try
            {
                KnownUserFactory.VerifyMd5Hash(SharedSecreteEventKey);

                Assert.Fail();
            }
            catch (InvalidKnownUserUrlException ex)
            {
                Assert.AreEqual("http://q.queue-it.net/inqueue.aspx", ex.OriginalUrl.AbsoluteUri.ToString());
            }
            catch (Exception)
            {
                Assert.Fail();
            }
        }

    }
}
