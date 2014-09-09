using System;
using System.IO;
using System.Linq;
using System.Web;
using System.Web.SessionState;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Rhino.Mocks;

namespace QueueIT.Security.Tests
{
    [TestClass]
    public class SessionValidateResultRepositoryTest
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

            var sessionContainer = new HttpSessionStateContainer("id", new SessionStateItemCollection(),
                                                    new HttpStaticObjectsCollection(), 10, true,
                                                    HttpCookieMode.AutoDetect,
                                                    SessionStateMode.InProc, false);
            SessionStateUtility.AddHttpSessionStateToContext(HttpContext.Current, sessionContainer);

            SessionValidateResultRepository.Clear();
        }

        [TestMethod]
        public void SessionValidateResultRepository_SetValidationResult_Test()
        {
            DateTime testOffest = DateTime.UtcNow;
            string sessionKey = "QueueITAccepted-SDFrts345E-customerid-eventid";

            Guid expectedQueueId = Guid.NewGuid();

            this._knownUser.Stub(knownUser => knownUser.CustomerId).Return("CustomerId");
            this._knownUser.Stub(knownUser => knownUser.EventId).Return("EventId");
            this._knownUser.Stub(knownUser => knownUser.QueueId).Return(expectedQueueId);
            this._knownUser.Stub(knownUser => knownUser.OriginalUrl).Return(new Uri("http://original.url/"));
            this._knownUser.Stub(knownUser => knownUser.PlaceInQueue).Return(5486);
            this._knownUser.Stub(knownUser => knownUser.RedirectType).Return(RedirectType.Queue);
            this._knownUser.Stub(knownUser => knownUser.TimeStamp).Return(testOffest);

            this._queue.Stub(queue => queue.CustomerId).Return("CustomerId");
            this._queue.Stub(queue => queue.EventId).Return("EventId");

            AcceptedConfirmedResult result = new AcceptedConfirmedResult(this._queue, this._knownUser, true);

            SessionValidateResultRepository repository = new SessionValidateResultRepository();

            repository.SetValidationResult(this._queue, result);

            var actualSessionState = HttpContext.Current.Session[sessionKey] as SessionStateModel;

            Assert.IsTrue(actualSessionState != null);
            Assert.AreEqual("http://original.url/", actualSessionState.OriginalUri);
            Assert.AreEqual(5486, actualSessionState.PlaceInQueue);
            Assert.AreEqual(expectedQueueId, actualSessionState.QueueId);
            Assert.AreEqual(RedirectType.Queue, actualSessionState.RedirectType);
            Assert.AreEqual(testOffest, actualSessionState.TimeStamp);
            Assert.IsNull(actualSessionState.Expiration);
        }

        [TestMethod]
        public void SessionValidateResultRepository_SetValidationResult_WithExpiration_Test()
        {
            DateTime testOffest = DateTime.UtcNow;
            string sessionKey = "QueueITAccepted-SDFrts345E-customerid-eventid";
            DateTime expectedExpiration = DateTime.UtcNow.AddMinutes(5);

            this._knownUser.Stub(knownUser => knownUser.CustomerId).Return("CustomerId");
            this._knownUser.Stub(knownUser => knownUser.EventId).Return("EventId");
            this._knownUser.Stub(knownUser => knownUser.QueueId).Return(Guid.NewGuid());
            this._knownUser.Stub(knownUser => knownUser.OriginalUrl).Return(new Uri("http://original.url/"));
            this._knownUser.Stub(knownUser => knownUser.PlaceInQueue).Return(5486);
            this._knownUser.Stub(knownUser => knownUser.RedirectType).Return(RedirectType.Idle);
            this._knownUser.Stub(knownUser => knownUser.TimeStamp).Return(testOffest);

            this._queue.Stub(queue => queue.CustomerId).Return("CustomerId");
            this._queue.Stub(queue => queue.EventId).Return("EventId");

            AcceptedConfirmedResult result = new AcceptedConfirmedResult(this._queue, this._knownUser, true);

            SessionValidateResultRepository repository = new SessionValidateResultRepository();

            repository.SetValidationResult(this._queue, result, expectedExpiration);

            var actualSessionState = HttpContext.Current.Session[sessionKey] as SessionStateModel;

            Assert.IsTrue(actualSessionState != null);
            Assert.IsTrue(actualSessionState.Expiration.HasValue);
            Assert.AreEqual(expectedExpiration, actualSessionState.Expiration.Value);
        }

        [TestMethod]
        public void SessionValidateResultRepository_SetValidationResult_IdleMode_WithExpiration_Test()
        {
            DateTime testOffest = DateTime.UtcNow;
            string sessionKey = "QueueITAccepted-SDFrts345E-customerid-eventid";

            this._knownUser.Stub(knownUser => knownUser.CustomerId).Return("CustomerId");
            this._knownUser.Stub(knownUser => knownUser.EventId).Return("EventId");
            this._knownUser.Stub(knownUser => knownUser.QueueId).Return(Guid.NewGuid());
            this._knownUser.Stub(knownUser => knownUser.OriginalUrl).Return(new Uri("http://original.url/"));
            this._knownUser.Stub(knownUser => knownUser.PlaceInQueue).Return(5486);
            this._knownUser.Stub(knownUser => knownUser.RedirectType).Return(RedirectType.Idle);
            this._knownUser.Stub(knownUser => knownUser.TimeStamp).Return(testOffest);

            this._queue.Stub(queue => queue.CustomerId).Return("CustomerId");
            this._queue.Stub(queue => queue.EventId).Return("EventId");

            AcceptedConfirmedResult result = new AcceptedConfirmedResult(this._queue, this._knownUser, true);

            SessionValidateResultRepository repository = new SessionValidateResultRepository();

            repository.SetValidationResult(this._queue, result);

            var actualSessionState = HttpContext.Current.Session[sessionKey] as SessionStateModel;

            Assert.IsTrue(actualSessionState != null);
            Assert.IsTrue(actualSessionState.Expiration.HasValue);
            Assert.IsTrue(testOffest.AddMinutes(3) <= actualSessionState.Expiration.Value);
            Assert.IsTrue(DateTime.UtcNow.AddMinutes(3) >= actualSessionState.Expiration.Value);
        }

        [TestMethod]
        public void SessionValidateResultRepository_SetValidationResult_DisabledMode_WithExpiration_Test()
        {
            DateTime testOffest = DateTime.UtcNow;
            string sessionKey = "QueueITAccepted-SDFrts345E-customerid-eventid";

            this._knownUser.Stub(knownUser => knownUser.CustomerId).Return("CustomerId");
            this._knownUser.Stub(knownUser => knownUser.EventId).Return("EventId");
            this._knownUser.Stub(knownUser => knownUser.QueueId).Return(Guid.NewGuid());
            this._knownUser.Stub(knownUser => knownUser.OriginalUrl).Return(new Uri("http://original.url/"));
            this._knownUser.Stub(knownUser => knownUser.PlaceInQueue).Return(5486);
            this._knownUser.Stub(knownUser => knownUser.RedirectType).Return(RedirectType.Disabled);
            this._knownUser.Stub(knownUser => knownUser.TimeStamp).Return(testOffest);

            this._queue.Stub(queue => queue.CustomerId).Return("CustomerId");
            this._queue.Stub(queue => queue.EventId).Return("EventId");

            AcceptedConfirmedResult result = new AcceptedConfirmedResult(this._queue, this._knownUser, true);

            SessionValidateResultRepository repository = new SessionValidateResultRepository();

            repository.SetValidationResult(this._queue, result);

            var actualSessionState = HttpContext.Current.Session[sessionKey] as SessionStateModel;

            Assert.IsTrue(actualSessionState != null);
            Assert.IsTrue(actualSessionState.Expiration.HasValue);
            Assert.IsTrue(testOffest.AddMinutes(3) <= actualSessionState.Expiration.Value);
            Assert.IsTrue(DateTime.UtcNow.AddMinutes(3) >= actualSessionState.Expiration.Value);
        }

        [TestMethod]
        public void SessionValidateResultRepository_GetValidationResult_Test()
        {
            string sessionKey = "QueueITAccepted-SDFrts345E-customerid-eventid";

            SessionStateModel model = new SessionStateModel()
            {
                OriginalUri = "http://original.url/",
                PlaceInQueue = 5486,
                QueueId = Guid.NewGuid(),
                RedirectType = RedirectType.Queue,
                TimeStamp = DateTime.UtcNow
            };
            HttpContext.Current.Session[sessionKey] = model;

            this._queue.Stub(queue => queue.CustomerId).Return("CustomerId");
            this._queue.Stub(queue => queue.EventId).Return("EventId");

            SessionValidateResultRepository repository = new SessionValidateResultRepository();

            AcceptedConfirmedResult actualValidationResult = repository.GetValidationResult(this._queue) 
                as AcceptedConfirmedResult;

            Assert.IsNotNull(actualValidationResult);
            Assert.AreEqual(new Uri(model.OriginalUri), actualValidationResult.KnownUser.OriginalUrl);
            Assert.AreEqual(model.PlaceInQueue, actualValidationResult.KnownUser.PlaceInQueue);
            Assert.AreEqual(model.QueueId, actualValidationResult.KnownUser.QueueId);
            Assert.AreEqual(model.RedirectType, actualValidationResult.KnownUser.RedirectType);
            Assert.AreEqual(model.TimeStamp, actualValidationResult.KnownUser.TimeStamp);
            Assert.IsFalse(actualValidationResult.IsInitialValidationRequest);
            Assert.AreSame(this._queue, actualValidationResult.Queue);
        }

        [TestMethod]
        public void SessionValidateResultRepository_GetValidationResult_Expired_Test()
        {
            string sessionKey = "QueueITAccepted-SDFrts345E-customerid-eventid";

            SessionStateModel model = new SessionStateModel()
            {
                OriginalUri = "http://original.url/",
                PlaceInQueue = 5486,
                QueueId = Guid.NewGuid(),
                RedirectType = RedirectType.Queue,
                TimeStamp = DateTime.UtcNow,
                Expiration = DateTime.UtcNow.AddMinutes(-1)
            };
            HttpContext.Current.Session[sessionKey] = model;

            this._queue.Stub(queue => queue.CustomerId).Return("CustomerId");
            this._queue.Stub(queue => queue.EventId).Return("EventId");

            SessionValidateResultRepository repository = new SessionValidateResultRepository();

            IValidateResult actualValidationResult = repository.GetValidationResult(this._queue);

            Assert.IsNull(actualValidationResult);
        }

        [TestMethod]
        public void SessionValidateResultRepository_Cancel_Test()
        {
            string sessionKey = "QueueITAccepted-SDFrts345E-customerid-eventid";

            SessionStateModel model = new SessionStateModel()
            {
                OriginalUri = "http://original.url/",
                PlaceInQueue = 5486,
                QueueId = Guid.NewGuid(),
                RedirectType = RedirectType.Queue,
                TimeStamp = DateTime.UtcNow
            };
            HttpContext.Current.Session[sessionKey] = model;

            this._queue.Stub(queue => queue.CustomerId).Return("CustomerId");
            this._queue.Stub(queue => queue.EventId).Return("EventId");

            SessionValidateResultRepository repository = new SessionValidateResultRepository();
            
            AcceptedConfirmedResult actualValidationResult = repository.GetValidationResult(this._queue)
                as AcceptedConfirmedResult;
            repository.Cancel(this._queue, actualValidationResult);

            Assert.IsNull(HttpContext.Current.Session[sessionKey]);
        }
    }
}
