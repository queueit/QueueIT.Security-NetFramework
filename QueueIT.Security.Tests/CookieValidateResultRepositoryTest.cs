using System;
using System.IO;
using System.Web;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Rhino.Mocks;

namespace QueueIT.Security.Tests
{
    [TestClass]
    public class CookieValidateResultRepositoryTest
    {
        private IQueue _queue;
        private IKnownUser _knownUser;
        private HttpResponse _response;
        private HttpRequest _request;

        [TestInitialize]
        public void TestInit()
        {
            this._queue = MockRepository.GenerateMock<IQueue>();
            this._knownUser = MockRepository.GenerateMock<IKnownUser>();
            this._request = new HttpRequest("test.aspx", "http://test.com/test.aspx", null);
            this._response = new HttpResponse(new StringWriter());
            HttpContext.Current = new HttpContext(this._request, this._response);

            CookieValidateResultRepository.Clear();
        }

        [TestMethod]
        public void CookieValidateResultRepository_GetValidationResult_ReadCookie_Test()
        {
            string secretKey = "acb";

            string expectedCustomerId = "CustomerId";
            string expectedEventId = "EventId";
            Guid expectedQueueId = new Guid(4567846, 35, 87, 3, 5, 8, 6, 4, 8, 2, 3);
            Uri expectedOriginalUrl = new Uri("http://original.url/");
            int expectedPlaceInQueue = 5486;
            RedirectType expectedRedirectType = RedirectType.Queue;
            long expectedSecondsSince1970 = 5465468;
            DateTime expectedTimeStamp = new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc).AddSeconds(expectedSecondsSince1970);
            string cookieName = "QueueITAccepted-SDFrts345E-" + expectedCustomerId.ToLower() + "-" + expectedEventId.ToLower();
            string expectedHash = "D5-48-23-FE-D0-42-D0-59-88-39-AB-D0-CA-A0-18-5D-B8-21-2C-A7-62-A9-65-73-62-68-74-C5-1C-50-09-BA";

            this._queue.Stub(queue => queue.CustomerId).Return(expectedCustomerId);
            this._queue.Stub(queue => queue.EventId).Return(expectedEventId);

            HttpCookie cookie = new HttpCookie(cookieName);
            cookie.Values["QueueId"] = expectedQueueId.ToString();
            cookie.Values["OriginalUrl"] = expectedOriginalUrl.AbsoluteUri;
            cookie.Values["PlaceInQueue"] = Hashing.EncryptPlaceInQueue(expectedPlaceInQueue);
            cookie.Values["RedirectType"] = expectedRedirectType.ToString();
            cookie.Values["TimeStamp"] = expectedSecondsSince1970.ToString();
            cookie.Values["Hash"] = expectedHash;
            
            this._request.Cookies.Add(cookie);

            KnownUserFactory.Configure(secretKey);

            CookieValidateResultRepository repository = new CookieValidateResultRepository();

            AcceptedConfirmedResult actualResult = repository.GetValidationResult(this._queue) as AcceptedConfirmedResult;

            Assert.IsNotNull(actualResult);
            Assert.AreEqual(this._queue, actualResult.Queue);
            Assert.AreEqual(expectedCustomerId, actualResult.KnownUser.CustomerId);
            Assert.AreEqual(expectedEventId, actualResult.KnownUser.EventId);
            Assert.AreEqual(expectedQueueId, actualResult.KnownUser.QueueId);
            Assert.AreEqual(expectedOriginalUrl, actualResult.KnownUser.OriginalUrl);
            Assert.AreEqual(expectedPlaceInQueue, actualResult.KnownUser.PlaceInQueue);
            Assert.AreEqual(expectedRedirectType, actualResult.KnownUser.RedirectType);
            Assert.AreEqual(expectedTimeStamp, actualResult.KnownUser.TimeStamp);
        }

        [TestMethod]
        public void CookieValidateResultRepository_GetValidationResult_RenewCookie_Test()
        {
            string secretKey = "acb";

            string expectedCustomerId = "CustomerId";
            string expectedEventId = "EventId";
            Guid expectedQueueId = new Guid(4567846, 35, 87, 3, 5, 8, 6, 4, 8, 2, 3);
            Uri expectedOriginalUrl = new Uri("http://original.url/");
            int expectedPlaceInQueue = 5486;
            RedirectType expectedRedirectType = RedirectType.Queue;
            long expectedSecondsSince1970 = 5465468;
            DateTime expectedTimeStamp = new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc).AddSeconds(expectedSecondsSince1970);
            string cookieName = "QueueITAccepted-SDFrts345E-" + expectedCustomerId.ToLower() + "-" + expectedEventId.ToLower();
            string expectedHash = "D5-48-23-FE-D0-42-D0-59-88-39-AB-D0-CA-A0-18-5D-B8-21-2C-A7-62-A9-65-73-62-68-74-C5-1C-50-09-BA";

            this._queue.Stub(queue => queue.CustomerId).Return(expectedCustomerId);
            this._queue.Stub(queue => queue.EventId).Return(expectedEventId);

            HttpCookie cookie = new HttpCookie(cookieName);
            cookie.Values["QueueId"] = expectedQueueId.ToString();
            cookie.Values["OriginalUrl"] = expectedOriginalUrl.AbsoluteUri;
            cookie.Values["PlaceInQueue"] = Hashing.EncryptPlaceInQueue(expectedPlaceInQueue);
            cookie.Values["RedirectType"] = expectedRedirectType.ToString();
            cookie.Values["TimeStamp"] = expectedSecondsSince1970.ToString();
            cookie.Values["Hash"] = expectedHash;

            this._request.Cookies.Add(cookie);

            KnownUserFactory.Configure(secretKey);

            CookieValidateResultRepository repository = new CookieValidateResultRepository();

            repository.GetValidationResult(this._queue);

            Assert.AreEqual(1, this._response.Cookies.Count);
        }

        [TestMethod]
        public void CookieValidateResultRepository_GetValidationResult_DisabledQueue_NoRenewCookie_Test()
        {
            string secretKey = "acb";

            string expectedCustomerId = "CustomerId";
            string expectedEventId = "EventId";
            Guid expectedQueueId = Guid.Empty;
            Uri expectedOriginalUrl = new Uri("http://original.url/");
            int expectedPlaceInQueue = 0;
            RedirectType expectedRedirectType = RedirectType.Disabled;
            long expectedSecondsSince1970 = 0;
            string cookieName = "QueueITAccepted-SDFrts345E-" + expectedCustomerId.ToLower() + "-" + expectedEventId.ToLower();
            string expectedHash = "FB-3C-5B-68-D9-85-7E-6F-4B-01-19-68-75-6B-31-AF-22-E0-C7-E3-C8-85-20-A4-64-46-95-6B-75-66-FE-9C";

            this._queue.Stub(queue => queue.CustomerId).Return(expectedCustomerId);
            this._queue.Stub(queue => queue.EventId).Return(expectedEventId);

            HttpCookie cookie = new HttpCookie(cookieName);
            cookie.Values["QueueId"] = expectedQueueId.ToString();
            cookie.Values["OriginalUrl"] = expectedOriginalUrl.AbsoluteUri;
            cookie.Values["PlaceInQueue"] = Hashing.EncryptPlaceInQueue(expectedPlaceInQueue);
            cookie.Values["RedirectType"] = expectedRedirectType.ToString();
            cookie.Values["TimeStamp"] = expectedSecondsSince1970.ToString();
            cookie.Values["Hash"] = expectedHash;

            this._request.Cookies.Add(cookie);

            KnownUserFactory.Configure(secretKey);

            CookieValidateResultRepository repository = new CookieValidateResultRepository();

            repository.GetValidationResult(this._queue);

            Assert.AreEqual(0, this._response.Cookies.Count);
        }

        [TestMethod]
        public void CookieValidateResultRepository_GetValidationResult_IdleQueue_NoRenewCookie_Test()
        {
            string secretKey = "acb";

            string expectedCustomerId = "CustomerId";
            string expectedEventId = "EventId";
            Guid expectedQueueId = Guid.Empty;
            Uri expectedOriginalUrl = new Uri("http://original.url/");
            int expectedPlaceInQueue = 0;
            RedirectType expectedRedirectType = RedirectType.Idle;
            long expectedSecondsSince1970 = 0;
            string cookieName = "QueueITAccepted-SDFrts345E-" + expectedCustomerId.ToLower() + "-" + expectedEventId.ToLower();
            string expectedHash = "17-77-3F-7D-2E-10-B1-F0-9B-41-5A-DD-37-BB-8E-3A-F7-0B-F2-9F-E3-3B-2B-F5-83-CE-88-C5-8C-15-26-B4";

            this._queue.Stub(queue => queue.CustomerId).Return(expectedCustomerId);
            this._queue.Stub(queue => queue.EventId).Return(expectedEventId);

            HttpCookie cookie = new HttpCookie(cookieName);
            cookie.Values["QueueId"] = expectedQueueId.ToString();
            cookie.Values["OriginalUrl"] = expectedOriginalUrl.AbsoluteUri;
            cookie.Values["PlaceInQueue"] = Hashing.EncryptPlaceInQueue(expectedPlaceInQueue);
            cookie.Values["RedirectType"] = expectedRedirectType.ToString();
            cookie.Values["TimeStamp"] = expectedSecondsSince1970.ToString();
            cookie.Values["Hash"] = expectedHash;

            this._request.Cookies.Add(cookie);

            KnownUserFactory.Configure(secretKey);

            CookieValidateResultRepository repository = new CookieValidateResultRepository();

            repository.GetValidationResult(this._queue);

            Assert.AreEqual(0, this._response.Cookies.Count);
        }

        [TestMethod]
        public void CookieValidateResultRepository_GetValidationResult_NoCookie_Test()
        {
            string secretKey = "acb";

            this._queue.Stub(queue => queue.CustomerId).Return("CustomerId");
            this._queue.Stub(queue => queue.EventId).Return("EventId");

            KnownUserFactory.Configure(secretKey);

            CookieValidateResultRepository repository = new CookieValidateResultRepository();

            IValidateResult actualResult = repository.GetValidationResult(this._queue);

            Assert.IsNull(actualResult);
        }

        [TestMethod]
        public void CookieValidateResultRepository_GetValidationResult_ModifiedCookie_Test()
        {
            string secretKey = "acb";

            string expectedCustomerId = "CustomerId";
            string expectedEventId = "EventId";
            Guid expectedQueueId = new Guid(4567846, 35, 87, 3, 5, 8, 6, 4, 8, 2, 3);
            Uri expectedOriginalUrl = new Uri("http://original.url/");
            int expectedPlaceInQueue = 5486;
            RedirectType expectedRedirectType = RedirectType.Queue;
            long expectedSecondsSince1970 = 5465468;
            DateTime expectedTimeStamp = new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc).AddSeconds(expectedSecondsSince1970);
            string cookieName = "QueueITAccepted-SDFrts345E-" + expectedCustomerId.ToLower() + "-" + expectedEventId.ToLower();
            string expectedHash = "D5-48-23-FE-D0-42-D0-59-88-39-AB-D0-CA-A0-18-5D-B8-21-2C-A7-62-A9-65-73-62-68-74-C5-1C-50-09-BA";

            this._queue.Stub(queue => queue.CustomerId).Return(expectedCustomerId);
            this._queue.Stub(queue => queue.EventId).Return(expectedEventId);

            HttpCookie cookie = new HttpCookie(cookieName);
            cookie.Values["QueueId"] = expectedQueueId.ToString();
            cookie.Values["OriginalUrl"] = expectedOriginalUrl.AbsoluteUri;
            cookie.Values["PlaceInQueue"] = Hashing.EncryptPlaceInQueue(expectedPlaceInQueue - 10);
            cookie.Values["RedirectType"] = expectedRedirectType.ToString();
            cookie.Values["TimeStamp"] = expectedSecondsSince1970.ToString();
            cookie.Values["Hash"] = expectedHash;

            this._request.Cookies.Add(cookie);

            KnownUserFactory.Configure(secretKey);

            CookieValidateResultRepository repository = new CookieValidateResultRepository();

            AcceptedConfirmedResult actualResult = repository.GetValidationResult(this._queue) as AcceptedConfirmedResult;

            Assert.IsNull(actualResult);
        }

        [TestMethod]
        public void CookieValidateResultRepository_SetValidationResult_WriteCookie_Test()
        {
            string secretKey = "acb";

            string expectedCustomerId = "CustomerId";
            string expectedEventId = "EventId";
            Guid expectedQueueId = new Guid(4567846,35,87,3,5,8,6,4,8,2,3);
            Uri expectedOriginalUrl = new Uri("http://original.url/");
            int expectedPlaceInQueue = 5486;
            RedirectType expectedRedirectType = RedirectType.Queue;
            long expectedSecondsSince1970 = 5465468;
            DateTime expectedTimeStamp = new DateTime(1970,1,1,0,0,0,DateTimeKind.Utc).AddSeconds(expectedSecondsSince1970);
            string expectedCookieName = "QueueITAccepted-SDFrts345E-" + expectedCustomerId.ToLower() + "-" + expectedEventId.ToLower();
            string expectedHash = "D5-48-23-FE-D0-42-D0-59-88-39-AB-D0-CA-A0-18-5D-B8-21-2C-A7-62-A9-65-73-62-68-74-C5-1C-50-09-BA";

            this._knownUser.Stub(knownUser => knownUser.CustomerId).Return(expectedCustomerId);
            this._knownUser.Stub(knownUser => knownUser.EventId).Return(expectedEventId);
            this._knownUser.Stub(knownUser => knownUser.QueueId).Return(expectedQueueId);
            this._knownUser.Stub(knownUser => knownUser.OriginalUrl).Return(expectedOriginalUrl);
            this._knownUser.Stub(knownUser => knownUser.PlaceInQueue).Return(expectedPlaceInQueue);
            this._knownUser.Stub(knownUser => knownUser.RedirectType).Return(expectedRedirectType);
            this._knownUser.Stub(knownUser => knownUser.TimeStamp).Return(expectedTimeStamp);

            this._queue.Stub(queue => queue.CustomerId).Return(expectedCustomerId);
            this._queue.Stub(queue => queue.EventId).Return(expectedEventId);

            CookieValidateResultRepository.Configure(null);
            KnownUserFactory.Configure(secretKey);

            CookieValidateResultRepository repository = new CookieValidateResultRepository();

            AcceptedConfirmedResult result = new AcceptedConfirmedResult(this._queue, this._knownUser, true);
            
            repository.SetValidationResult(this._queue, result);

            Assert.AreEqual(1, this._response.Cookies.Count);
            Assert.AreEqual(expectedCookieName, this._response.Cookies[0].Name);
            Assert.IsNull(this._response.Cookies[0].Domain);
            Assert.IsTrue(this._response.Cookies[0].HttpOnly);
            Assert.IsTrue(this._response.Cookies[0].Expires > DateTime.UtcNow.AddMinutes(19).AddSeconds(50));
            Assert.IsTrue(this._response.Cookies[0].Expires < DateTime.UtcNow.AddMinutes(20).AddSeconds(10));
            Assert.AreEqual(expectedQueueId.ToString(), this._response.Cookies[0]["QueueId"]);
            Assert.AreEqual(expectedSecondsSince1970.ToString(), this._response.Cookies[0]["TimeStamp"]);
            Assert.AreEqual(expectedRedirectType.ToString(), this._response.Cookies[0]["RedirectType"]);
            Assert.AreEqual(expectedPlaceInQueue, Hashing.DecryptPlaceInQueue(this._response.Cookies[0]["PlaceInQueue"]));
            Assert.AreEqual(expectedHash, this._response.Cookies[0]["Hash"]);
        }

        [TestMethod]
        public void CookieValidateResultRepository_SetValidationResult_WriteCookie_DisabledQueue_Expiration_Test()
        {
            string secretKey = "acb";

            string expectedCustomerId = "CustomerId";
            string expectedEventId = "EventId";
            Guid expectedQueueId = Guid.Empty;
            Uri expectedOriginalUrl = new Uri("http://original.url/");
            int expectedPlaceInQueue = 0;
            RedirectType expectedRedirectType = RedirectType.Disabled;
            long expectedSecondsSince1970 = 0;
            DateTime expectedTimeStamp = new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc).AddSeconds(expectedSecondsSince1970);
            string expectedCookieName = "QueueITAccepted-SDFrts345E-" + expectedCustomerId.ToLower() + "-" + expectedEventId.ToLower();

            this._knownUser.Stub(knownUser => knownUser.CustomerId).Return(expectedCustomerId);
            this._knownUser.Stub(knownUser => knownUser.EventId).Return(expectedEventId);
            this._knownUser.Stub(knownUser => knownUser.QueueId).Return(expectedQueueId);
            this._knownUser.Stub(knownUser => knownUser.OriginalUrl).Return(expectedOriginalUrl);
            this._knownUser.Stub(knownUser => knownUser.PlaceInQueue).Return(expectedPlaceInQueue);
            this._knownUser.Stub(knownUser => knownUser.RedirectType).Return(expectedRedirectType);
            this._knownUser.Stub(knownUser => knownUser.TimeStamp).Return(expectedTimeStamp);

            this._queue.Stub(queue => queue.CustomerId).Return(expectedCustomerId);
            this._queue.Stub(queue => queue.EventId).Return(expectedEventId);

            CookieValidateResultRepository.Configure(null);
            KnownUserFactory.Configure(secretKey);

            CookieValidateResultRepository repository = new CookieValidateResultRepository();

            AcceptedConfirmedResult result = new AcceptedConfirmedResult(this._queue, this._knownUser, true);

            repository.SetValidationResult(this._queue, result);

            Assert.AreEqual(1, this._response.Cookies.Count);
            Assert.AreEqual(expectedCookieName, this._response.Cookies[0].Name);
            Assert.IsTrue(this._response.Cookies[0].Expires > DateTime.UtcNow.AddMinutes(2).AddSeconds(50));
            Assert.IsTrue(this._response.Cookies[0].Expires < DateTime.UtcNow.AddMinutes(3).AddSeconds(10));
        }

        [TestMethod]
        public void CookieValidateResultRepository_SetValidationResult_WriteCookie_IdleQueue_Expiration_Test()
        {
            string secretKey = "acb";

            string expectedCustomerId = "CustomerId";
            string expectedEventId = "EventId";
            Guid expectedQueueId = Guid.Empty;
            Uri expectedOriginalUrl = new Uri("http://original.url/");
            int expectedPlaceInQueue = 0;
            RedirectType expectedRedirectType = RedirectType.Idle;
            long expectedSecondsSince1970 = 0;
            DateTime expectedTimeStamp = new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc).AddSeconds(expectedSecondsSince1970);
            string expectedCookieName = "QueueITAccepted-SDFrts345E-" + expectedCustomerId.ToLower() + "-" + expectedEventId.ToLower();

            this._knownUser.Stub(knownUser => knownUser.CustomerId).Return(expectedCustomerId);
            this._knownUser.Stub(knownUser => knownUser.EventId).Return(expectedEventId);
            this._knownUser.Stub(knownUser => knownUser.QueueId).Return(expectedQueueId);
            this._knownUser.Stub(knownUser => knownUser.OriginalUrl).Return(expectedOriginalUrl);
            this._knownUser.Stub(knownUser => knownUser.PlaceInQueue).Return(expectedPlaceInQueue);
            this._knownUser.Stub(knownUser => knownUser.RedirectType).Return(expectedRedirectType);
            this._knownUser.Stub(knownUser => knownUser.TimeStamp).Return(expectedTimeStamp);

            this._queue.Stub(queue => queue.CustomerId).Return(expectedCustomerId);
            this._queue.Stub(queue => queue.EventId).Return(expectedEventId);

            CookieValidateResultRepository.Configure(null);
            KnownUserFactory.Configure(secretKey);

            CookieValidateResultRepository repository = new CookieValidateResultRepository();

            AcceptedConfirmedResult result = new AcceptedConfirmedResult(this._queue, this._knownUser, true);

            repository.SetValidationResult(this._queue, result);

            Assert.AreEqual(1, this._response.Cookies.Count);
            Assert.AreEqual(expectedCookieName, this._response.Cookies[0].Name);
            Assert.IsTrue(this._response.Cookies[0].Expires > DateTime.UtcNow.AddMinutes(2).AddSeconds(50));
            Assert.IsTrue(this._response.Cookies[0].Expires < DateTime.UtcNow.AddMinutes(3).AddSeconds(10));
        }

        [TestMethod]
        public void CookieValidateResultRepository_SetValidationResult_WriteCookie_WithExpiration_Test()
        {
            DateTime expectedExpiration = DateTime.UtcNow.AddMinutes(5);
            string secretKey = "acb";

            string expectedCustomerId = "CustomerId";
            string expectedEventId = "EventId";
            Guid expectedQueueId = Guid.Empty;
            Uri expectedOriginalUrl = new Uri("http://original.url/");
            int expectedPlaceInQueue = 0;
            RedirectType expectedRedirectType = RedirectType.Idle;
            long expectedSecondsSince1970 = 0;
            DateTime expectedTimeStamp = new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc).AddSeconds(expectedSecondsSince1970);
            string expectedCookieName = "QueueITAccepted-SDFrts345E-" + expectedCustomerId.ToLower() + "-" + expectedEventId.ToLower();

            this._knownUser.Stub(knownUser => knownUser.CustomerId).Return(expectedCustomerId);
            this._knownUser.Stub(knownUser => knownUser.EventId).Return(expectedEventId);
            this._knownUser.Stub(knownUser => knownUser.QueueId).Return(expectedQueueId);
            this._knownUser.Stub(knownUser => knownUser.OriginalUrl).Return(expectedOriginalUrl);
            this._knownUser.Stub(knownUser => knownUser.PlaceInQueue).Return(expectedPlaceInQueue);
            this._knownUser.Stub(knownUser => knownUser.RedirectType).Return(expectedRedirectType);
            this._knownUser.Stub(knownUser => knownUser.TimeStamp).Return(expectedTimeStamp);

            this._queue.Stub(queue => queue.CustomerId).Return(expectedCustomerId);
            this._queue.Stub(queue => queue.EventId).Return(expectedEventId);

            CookieValidateResultRepository.Configure(null);
            KnownUserFactory.Configure(secretKey);

            CookieValidateResultRepository repository = new CookieValidateResultRepository();

            AcceptedConfirmedResult result = new AcceptedConfirmedResult(this._queue, this._knownUser, true);

            repository.SetValidationResult(this._queue, result, expectedExpiration);

            Assert.AreEqual(1, this._response.Cookies.Count);
            Assert.AreEqual(expectedCookieName, this._response.Cookies[0].Name);
            Assert.AreEqual(this._response.Cookies[0].Expires, expectedExpiration);
        }

        [TestMethod]
        public void CookieValidateResultRepository_SetValidationResult_CookieDomain_Test()
        {
            string secretKey = "acb";

            string expectedCookieDomain = ".mydomain.com";

            this._knownUser.Stub(knownUser => knownUser.CustomerId).Return("CustomerId");
            this._knownUser.Stub(knownUser => knownUser.EventId).Return("EventId");
            this._knownUser.Stub(knownUser => knownUser.QueueId).Return(Guid.NewGuid());
            this._knownUser.Stub(knownUser => knownUser.OriginalUrl).Return(new Uri("http://original.url/"));
            this._knownUser.Stub(knownUser => knownUser.PlaceInQueue).Return(5486);
            this._knownUser.Stub(knownUser => knownUser.RedirectType).Return(RedirectType.Queue);
            this._knownUser.Stub(knownUser => knownUser.TimeStamp).Return(DateTime.UtcNow);

            this._queue.Stub(queue => queue.CustomerId).Return("CustomerId");
            this._queue.Stub(queue => queue.EventId).Return("EventId");

            CookieValidateResultRepository.Configure(expectedCookieDomain);
            KnownUserFactory.Configure(secretKey);

            CookieValidateResultRepository repository = new CookieValidateResultRepository();

            AcceptedConfirmedResult result = new AcceptedConfirmedResult(this._queue, this._knownUser, true);

            repository.SetValidationResult(this._queue, result);

            Assert.AreEqual(1, this._response.Cookies.Count);
            Assert.AreEqual(expectedCookieDomain, this._response.Cookies[0].Domain);
        }

        [TestMethod]
        public void CookieValidateResultRepository_SetValidationResult_DefaultExpiration_Test()
        {
            DateTime testOffest = DateTime.UtcNow;

            string secretKey = "acb";

            this._knownUser.Stub(knownUser => knownUser.CustomerId).Return("CustomerId");
            this._knownUser.Stub(knownUser => knownUser.EventId).Return("EventId");
            this._knownUser.Stub(knownUser => knownUser.QueueId).Return(Guid.NewGuid());
            this._knownUser.Stub(knownUser => knownUser.OriginalUrl).Return(new Uri("http://original.url/"));
            this._knownUser.Stub(knownUser => knownUser.PlaceInQueue).Return(5486);
            this._knownUser.Stub(knownUser => knownUser.RedirectType).Return(RedirectType.Queue);
            this._knownUser.Stub(knownUser => knownUser.TimeStamp).Return(DateTime.UtcNow);

            this._queue.Stub(queue => queue.CustomerId).Return("CustomerId");
            this._queue.Stub(queue => queue.EventId).Return("EventId");

            KnownUserFactory.Configure(secretKey);

            CookieValidateResultRepository repository = new CookieValidateResultRepository();

            AcceptedConfirmedResult result = new AcceptedConfirmedResult(this._queue, this._knownUser, true);

            repository.SetValidationResult(this._queue, result);

            Assert.AreEqual(1, this._response.Cookies.Count);
            Assert.IsTrue(this._response.Cookies[0].Expires >= testOffest.AddMinutes(20) && 
                this._response.Cookies[0].Expires <= DateTime.UtcNow.AddMinutes(20));
        }

        [TestMethod]
        public void CookieValidateResultRepository_SetValidationResult_CookieExpiration_Test()
        {
            DateTime testOffest = DateTime.UtcNow;

            string secretKey = "acb";

            this._knownUser.Stub(knownUser => knownUser.CustomerId).Return("CustomerId");
            this._knownUser.Stub(knownUser => knownUser.EventId).Return("EventId");
            this._knownUser.Stub(knownUser => knownUser.QueueId).Return(Guid.NewGuid());
            this._knownUser.Stub(knownUser => knownUser.OriginalUrl).Return(new Uri("http://original.url/"));
            this._knownUser.Stub(knownUser => knownUser.PlaceInQueue).Return(5486);
            this._knownUser.Stub(knownUser => knownUser.RedirectType).Return(RedirectType.Queue);
            this._knownUser.Stub(knownUser => knownUser.TimeStamp).Return(DateTime.UtcNow);

            this._queue.Stub(queue => queue.CustomerId).Return("CustomerId");
            this._queue.Stub(queue => queue.EventId).Return("EventId");

            KnownUserFactory.Configure(secretKey);

            CookieValidateResultRepository.Configure(cookieExpiration: TimeSpan.FromMinutes(5));
            CookieValidateResultRepository repository = new CookieValidateResultRepository();

            AcceptedConfirmedResult result = new AcceptedConfirmedResult(this._queue, this._knownUser, true);

            repository.SetValidationResult(this._queue, result);

            Assert.AreEqual(1, this._response.Cookies.Count);
            Assert.IsTrue(this._response.Cookies[0].Expires >= testOffest.AddMinutes(5) &&
                this._response.Cookies[0].Expires <= DateTime.UtcNow.AddMinutes(5));
        }

        [TestMethod]
        public void CookieValidateResultRepository_SetValidationResult_NotAccepted_NoCookie_Test()
        {
            this._knownUser.Stub(knownUser => knownUser.CustomerId).Return("CustomerId");
            this._knownUser.Stub(knownUser => knownUser.EventId).Return("EventId");
            this._knownUser.Stub(knownUser => knownUser.QueueId).Return(Guid.NewGuid());
            this._knownUser.Stub(knownUser => knownUser.OriginalUrl).Return(new Uri("http://original.url/"));
            this._knownUser.Stub(knownUser => knownUser.PlaceInQueue).Return(5486);
            this._knownUser.Stub(knownUser => knownUser.RedirectType).Return(RedirectType.Queue);
            this._knownUser.Stub(knownUser => knownUser.TimeStamp).Return(DateTime.UtcNow);

            this._queue.Stub(queue => queue.CustomerId).Return("CustomerId");
            this._queue.Stub(queue => queue.EventId).Return("EventId");

            KnownUserFactory.Configure("acb");

            CookieValidateResultRepository repository = new CookieValidateResultRepository();

            EnqueueResult result = new EnqueueResult(this._queue, new Uri("http://q.queue-it.net/"));

            repository.SetValidationResult(this._queue, result);

            Assert.AreEqual(0, this._response.Cookies.Count);
        }

        [TestMethod]
        public void CookieValidateResultRepository_Cancel_Test()
        {
            DateTime testOffset = DateTime.UtcNow;
            string secretKey = "acb";

            string expectedCustomerId = "CustomerId";
            string expectedEventId = "EventId";
            Guid expectedQueueId = new Guid(4567846, 35, 87, 3, 5, 8, 6, 4, 8, 2, 3);
            Uri expectedOriginalUrl = new Uri("http://original.url/");
            int expectedPlaceInQueue = 5486;
            RedirectType expectedRedirectType = RedirectType.Queue;
            long expectedSecondsSince1970 = 5465468;
            DateTime expectedTimeStamp = new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc).AddSeconds(expectedSecondsSince1970);
            string cookieName = "QueueITAccepted-SDFrts345E-" + expectedCustomerId.ToLower() + "-" + expectedEventId.ToLower();
            string expectedHash = "D5-48-23-FE-D0-42-D0-59-88-39-AB-D0-CA-A0-18-5D-B8-21-2C-A7-62-A9-65-73-62-68-74-C5-1C-50-09-BA";

            this._queue.Stub(queue => queue.CustomerId).Return(expectedCustomerId);
            this._queue.Stub(queue => queue.EventId).Return(expectedEventId);

            HttpCookie cookie = new HttpCookie(cookieName);
            cookie.Values["QueueId"] = expectedQueueId.ToString();
            cookie.Values["OriginalUrl"] = expectedOriginalUrl.AbsoluteUri;
            cookie.Values["PlaceInQueue"] = Hashing.EncryptPlaceInQueue(expectedPlaceInQueue);
            cookie.Values["RedirectType"] = expectedRedirectType.ToString();
            cookie.Values["TimeStamp"] = expectedSecondsSince1970.ToString();
            cookie.Values["Hash"] = expectedHash;

            this._request.Cookies.Add(cookie);

            KnownUserFactory.Configure(secretKey);

            CookieValidateResultRepository repository = new CookieValidateResultRepository();

            AcceptedConfirmedResult actualResult = repository.GetValidationResult(this._queue) as AcceptedConfirmedResult;
            repository.Cancel(this._queue, actualResult);

            Assert.AreEqual(1, this._response.Cookies.Count);
            Assert.IsTrue(this._response.Cookies[0].Expires >= testOffset.AddDays(-1) &&
                this._response.Cookies[0].Expires <= DateTime.UtcNow.AddDays(-1));
        }
    }
}
