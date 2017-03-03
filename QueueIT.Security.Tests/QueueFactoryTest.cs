using System;
using System.Globalization;
using System.Web;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace QueueIT.Security.Tests
{
    [TestClass]
    public class QueueFactoryTest
    {
        
        [TestInitialize]
        public void TestInitialize()
        {
            QueueFactory.Reset();
            QueueFactory.Configure();
        }


        [TestMethod]
        public void QueueFactory_CreateQueue_Constructor_Test()
        {
            string expectedCustomerId = "customerid";
            string expectedEventId = "eventid";

            IQueue queue = QueueFactory.CreateQueue(expectedCustomerId, expectedEventId);

            Assert.AreEqual(expectedCustomerId, queue.CustomerId);
            Assert.AreEqual(expectedEventId, queue.EventId);
        }

        [TestMethod]
        public void QueueFactory_CreateQueue_Constructor_Configuration_Default_Test()
        {
            QueueFactory.Reset();

            string expectedCustomerId = "defaultcustomerid";
            string expectedEventId = "defaulteventid";

            IQueue queue = QueueFactory.CreateQueue();

            Assert.AreEqual(expectedCustomerId, queue.CustomerId);
            Assert.AreEqual(expectedEventId, queue.EventId);
        }

        [TestMethod]
        public void QueueFactory_CreateQueue_Constructor_Configuration_Named_Test()
        {
            QueueFactory.Reset();

            string expectedCustomerId = "queue1customerid";
            string expectedEventId = "queue1eventid";

            IQueue queue = QueueFactory.CreateQueue("queue1");

            Assert.AreEqual(expectedCustomerId, queue.CustomerId);
            Assert.AreEqual(expectedEventId, queue.EventId);
        }

        [TestMethod]
        public void QueueFactory_GetQueueUrl_Test()
        {
            string expectedCustomerId = "customerid";
            string expectedEventId = "eventid";

            string expectedQueueUrl =
                "http://" + expectedCustomerId + ".queue-it.net/?c=" + expectedCustomerId + "&e=" + expectedEventId;

            IQueue queue = QueueFactory.CreateQueue(expectedCustomerId, expectedEventId);

            string actualQueueUrl = queue.GetQueueUrl();

            Assert.IsTrue(actualQueueUrl.StartsWith(expectedQueueUrl));
        }

        [TestMethod]
        public void QueueFactory_GetQueueUrl_Culture_Test()
        {
            string expectedCustomerId = "customerid";
            string expectedEventId = "eventid";
            string expectedCulture = "en-US";

            string expectedQueueUrl =
                "&cid=" + expectedCulture;

            IQueue queue = QueueFactory.CreateQueue(expectedCustomerId, expectedEventId);

            string actualQueueUrl = queue.GetQueueUrl(language: new CultureInfo(expectedCulture));

            Assert.IsTrue(actualQueueUrl.Contains(expectedQueueUrl));
        }

        [TestMethod]
        public void QueueFactory_GetQueueUrl_Version_Test()
        {
            string expectedCustomerId = "customerid";
            string expectedEventId = "eventid";
            string expectedVersion = "c" + typeof(Queue).Assembly.GetName().Version.ToString();

            string expectedQueueUrl = "&ver=" + expectedVersion;

            IQueue queue = QueueFactory.CreateQueue(expectedCustomerId, expectedEventId);

            string actualQueueUrl = queue.GetQueueUrl();

            Assert.IsTrue(actualQueueUrl.Contains(expectedQueueUrl));
        }

        [TestMethod]
        public void QueueFactory_GetQueueUrl_LayoutName_Test()
        {
            string expectedCustomerId = "customerid";
            string expectedEventId = "eventid";
            string expectedLayoutName = "Some Other Layout";

            string expectedQueueUrl = "&l=" + HttpUtility.UrlEncode(expectedLayoutName);

            IQueue queue = QueueFactory.CreateQueue(expectedCustomerId, expectedEventId);

            string actualQueueUrl = queue.GetQueueUrl(layoutName: expectedLayoutName);

            Assert.IsTrue(actualQueueUrl.Contains(expectedQueueUrl));
        }

        [TestMethod]
        public void QueueFactory_GetQueueUrl_DomainAlias_Test()
        {
            string expectedCustomerId = "customerid";
            string expectedEventId = "eventid";
            string expectedDomainAlias = "my.queue.url";

            string expectedQueueUrl =
                "http://" + expectedDomainAlias + "/";

            IQueue queue = QueueFactory.CreateQueue("customerId", "eventId");

            string actualQueueUrl = queue.GetQueueUrl(domainAlias: expectedDomainAlias);

            Assert.IsTrue(actualQueueUrl.StartsWith(expectedQueueUrl));
        }

        [TestMethod]
        public void QueueFactory_GetQueueUrl_Ssl_Test()
        {
            string expectedCustomerId = "customerid";
            string expectedEventId = "eventid";

            string expectedQueueUrl =
                "https://" + expectedCustomerId + ".queue-it.net/?c=" + expectedCustomerId + "&e=" + expectedEventId;

            IQueue queue = QueueFactory.CreateQueue(expectedCustomerId, expectedEventId);

            string actualQueueUrl = queue.GetQueueUrl(sslEnabled: true);

            Assert.IsTrue(actualQueueUrl.StartsWith(expectedQueueUrl));
        }

        [TestMethod]
        public void QueueFactory_GetQueueUrl_IncludeTarget_Test()
        {
            string expectedCustomerId = "customerid";
            string expectedEventId = "eventid";
            string expectedTarget = "http://target.url/?someprop=somevalue&another=value";

            string expectedQueueUrl = "&t=" + HttpUtility.UrlEncode(expectedTarget);

            KnownUserFactory.Configure(urlProviderFactory: () => new MockKnownUserUrlProvicer(expectedTarget));

            IQueue queue = QueueFactory.CreateQueue(expectedCustomerId, expectedEventId);

            string actualQueueUrl = queue.GetQueueUrl(includeTargetUrl: true);

            Assert.IsTrue(actualQueueUrl.Contains(expectedQueueUrl));
        }

        [TestMethod]
        [Ignore]
        public void QueueFactory_GetQueueUrl_IncludeTargetWithInvalidChars_Test()
        {
            string expectedCustomerId = "customerid";
            string expectedEventId = "eventid";
            string expectedTarget = "http://target.url/?someprop=some|value&another={value}";

            string expectedQueueUrl = "&t=" + HttpUtility.UrlEncode(expectedTarget);

            HttpContext.Current = new HttpContext(
                new HttpRequest("", expectedTarget, "someprop=some|value&another={value}"),
                new HttpResponse(null));

            IQueue queue = QueueFactory.CreateQueue(expectedCustomerId, expectedEventId);

            string actualQueueUrl = queue.GetQueueUrl(includeTargetUrl: true);

            Assert.IsTrue(actualQueueUrl.Contains(expectedQueueUrl));
        }

        [TestMethod]
        public void QueueFactory_GetQueueUrl_TargetUrl_Test()
        {
            string expectedCustomerId = "customerid";
            string expectedEventId = "eventid";
            string expectedTarget = "http://target.url/?someprop=somevalue&another=value";

            string expectedQueueUrl = "&t=" + HttpUtility.UrlEncode(expectedTarget);

            IQueue queue = QueueFactory.CreateQueue(expectedCustomerId, expectedEventId);

            string actualQueueUrl = queue.GetQueueUrl(expectedTarget);

            Assert.IsTrue(actualQueueUrl.Contains(expectedQueueUrl));
        }

        [TestMethod]
        public void QueueFactory_GetCancelUrl_Test()
        {
            string expectedCustomerId = "customerid";
            string expectedEventId = "eventid";

            string expectedCancelUrl =
                "http://" + expectedCustomerId + ".queue-it.net/cancel.aspx?c=" + expectedCustomerId + "&e=" + expectedEventId;

            IQueue queue = QueueFactory.CreateQueue(expectedCustomerId, expectedEventId);

            string actualQueueUrl = queue.GetCancelUrl();

            Assert.AreEqual(expectedCancelUrl, actualQueueUrl);
        }

        [TestMethod]
        public void QueueFactory_GetCancelUrl_Ssl_Test()
        {
            string expectedCustomerId = "customerid";
            string expectedEventId = "eventid";

            string expectedCancelUrl =
                "https://" + expectedCustomerId + ".queue-it.net/cancel.aspx?c=" + expectedCustomerId + "&e=" + expectedEventId;

            IQueue queue = QueueFactory.CreateQueue(expectedCustomerId, expectedEventId);

            string actualQueueUrl = queue.GetCancelUrl(sslEnabled: true);

            Assert.AreEqual(expectedCancelUrl, actualQueueUrl);
        }

        [TestMethod]
        public void QueueFactory_GetCancelUrl_DomainAlias_Test()
        {
            string expectedCustomerId = "customerid";
            string expectedEventId = "eventid";

            string expectedCancelUrl =
                "http://vent.queue-it.net/cancel.aspx?c=" + expectedCustomerId + "&e=" + expectedEventId;

            IQueue queue = QueueFactory.CreateQueue(expectedCustomerId, expectedEventId);

            string actualQueueUrl = queue.GetCancelUrl(domainAlias: "vent.queue-it.net");

            Assert.AreEqual(expectedCancelUrl, actualQueueUrl);
        }


        [TestMethod]
        public void QueueFactory_GetCancelUrl_ConfigurationSection_Test()
        {
            QueueFactory.Reset();

            string expectedCustomerId = "defaultcustomerid";
            string expectedEventId = "defaulteventid";

            string expectedCancelUrl =
                "http://" + expectedCustomerId + ".queue-it.net/cancel.aspx?c=" + expectedCustomerId + "&e=" + expectedEventId;

            IQueue queue = QueueFactory.CreateQueue();

            string actualQueueUrl = queue.GetCancelUrl();

            Assert.AreEqual(expectedCancelUrl, actualQueueUrl);
        }

        [TestMethod]
        public void QueueFactory_GetCancelUrl_ConfigurationSection_Named_Test()
        {
            QueueFactory.Reset();

            string expectedCustomerId = "queue1customerid";
            string expectedEventId = "queue1eventid";

            string expectedCancelUrl =
                "https://queue.mala.dk/cancel.aspx?c=" + expectedCustomerId + "&e=" + expectedEventId + "&r=http%3a%2f%2fwww.mysplitpage.com%2f";

            IQueue queue = QueueFactory.CreateQueue("queue1");

            string actualQueueUrl = queue.GetCancelUrl();

            Assert.AreEqual(expectedCancelUrl, actualQueueUrl);
        }


        [TestMethod]
        public void QueueFactory_GetCancelUrl_LandingPage_Test()
        {
            string expectedCustomerId = "customerid";
            string expectedEventId = "eventid";
            string expectedTarget = "http://target.url/?someprop=somevalue&another=value";

            string expectedCancelUrl =
                "http://" + expectedCustomerId + ".queue-it.net/cancel.aspx?c=" + expectedCustomerId + "&e=" + expectedEventId +
                "&r=" + HttpUtility.UrlEncode(expectedTarget);

            IQueue queue = QueueFactory.CreateQueue(expectedCustomerId, expectedEventId);

            string actualQueueUrl = queue.GetCancelUrl(expectedTarget);

            Assert.AreEqual(expectedCancelUrl, actualQueueUrl);
        }

        [TestMethod]
        public void QueueFactory_GetCancelUrl_Configure_Test()
        {
            QueueFactory.Configure(hostDomain: "testq.queue-it.net");

            string expectedCustomerId = "customerid";
            string expectedEventId = "eventid";

            string expectedCancelUrl =
                "http://" + expectedCustomerId + ".testq.queue-it.net/cancel.aspx?c=" + expectedCustomerId + "&e=" + expectedEventId;

            IQueue queue = QueueFactory.CreateQueue(expectedCustomerId, expectedEventId);

            string actualQueueUrl = queue.GetCancelUrl();

            Assert.AreEqual(expectedCancelUrl, actualQueueUrl);
        }

        [TestMethod]
        public void QueueFactory_GetLandingPageUrl_Test()
        {
            IQueue queue = QueueFactory.CreateQueue("customerid", "eventid");

            string actualLandingPageUrl = queue.GetLandingPageUrl();

            Assert.IsNull(actualLandingPageUrl);
        }

        [TestMethod]
        public void QueueFactory_GetLandingPageUrl_Configuration_Test()
        {
            string expectedLandingPageUrl = "http://www.mysplitpage.com/";

            HttpContext.Current = new HttpContext(
                new HttpRequest("", expectedLandingPageUrl, ""),
                new HttpResponse(null));

            IQueue queue = QueueFactory.CreateQueue("queue1");

            string actualLandingPageUrl = queue.GetLandingPageUrl(false);

            Assert.AreEqual(expectedLandingPageUrl, actualLandingPageUrl);
        }

        [TestMethod]
        public void QueueFactory_GetLandingPageUrl_IncludeTargetUrl_Test()
        {
            string expectedTarget = "http://target.url/?someprop=somevalue&another=value";
            string expectedLandingPageUrl = "http://www.mysplitpage.com/?t=" + HttpUtility.UrlEncode(expectedTarget);

            HttpContext.Current = new HttpContext(
                new HttpRequest("", expectedTarget, "someprop=somevalue&another=value"),
                new HttpResponse(null));

            IQueue queue = QueueFactory.CreateQueue("queue1");

            string actualLandingPageUrl = queue.GetLandingPageUrl(true);

            Assert.AreEqual(expectedLandingPageUrl, actualLandingPageUrl);
        }

        [TestMethod]
        public void QueueFactory_GetLandingPageUrl_TargetUrl_Test()
        {
            string expectedTarget = "http://target.url/?someprop=somevalue&another=value";
            string expectedLandingPageUrl = "http://www.mysplitpage.com/?t=" + HttpUtility.UrlEncode(expectedTarget);

            IQueue queue = QueueFactory.CreateQueue("queue1");

            string actualLandingPageUrl = queue.GetLandingPageUrl(expectedTarget);

            Assert.AreEqual(expectedLandingPageUrl, actualLandingPageUrl);
        }

        private class MockKnownUserUrlProvicer : IKnownUserUrlProvider
        {
            private readonly string _url;
            public MockKnownUserUrlProvicer(string url)
            {
                _url = url;
            }

            public string GetUrl()
            {
                return this._url;
            }

            public string GetQueueId(string queryStringPrefix)
            {
                throw new NotImplementedException();
            }

            public string GetPlaceInQueue(string queryStringPrefix)
            {
                throw new NotImplementedException();
            }

            public string GetTimeStamp(string queryStringPrefix)
            {
                throw new NotImplementedException();
            }

            public string GetEventId(string queryStringPrefix)
            {
                throw new NotImplementedException();
            }

            public string GetCustomerId(string queryStringPrefix)
            {
                throw new NotImplementedException();
            }

            public string GetOriginalUrl(string queryStringPrefix)
            {
                throw new NotImplementedException();
            }

            public string GetRedirectType(string queryStringPrefix)
            {
                throw new NotImplementedException();
            }
        }
    }
}