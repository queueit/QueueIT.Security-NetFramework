using System;
using System.Collections.Generic;
using System.Web;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace QueueIT.Security.Tests
{
    [TestClass]
    public class SessionValidationControllerTest
    {
        private IValidateResultRepository _resultRepository;
        private const string SharedSecreteEventKey =
            "zaqxswcdevfrbgtnhymjukiloZAQCDEFRBGTNHYMJUKILOPlkjhgfdsapoiuytrewqmnbvcx";

        [TestInitialize]
        public void TestInitialize()
        {
            this._resultRepository = new MockValidationResultRepository();

            KnownUserFactory.Reset(false);
            KnownUserFactory.Configure(secretKey: SharedSecreteEventKey);
            QueueFactory.Reset();
            QueueFactory.Configure();
            SessionValidationController.Configure(validationResultProviderFactory: () => this._resultRepository);

            HttpContext.Current = new HttpContext(
                new HttpRequest("", "http://some.url", "someprop=somevalue&another=value"),
                new HttpResponse(null));
        }

        [TestMethod]
        public void SessionValidationController_ValidateRequest_Test()
        {
            IValidateResult result = SessionValidationController.ValidateRequest(QueueFactory.CreateQueue("customerId", "eventId"));

            Assert.IsInstanceOfType(result, typeof(EnqueueResult));
        }

        [TestMethod]
        public void SessionValidationController_ValidateRequest_KnownUserAccepted_Test()
        {
            KnownUserFactory.Reset(false);
            KnownUserFactory.Configure(SharedSecreteEventKey);

            int expectedPlaceInqueue = 7810;
            Guid expectedQueueId = Guid.NewGuid();
            string placeInQueueEncrypted = Hashing.EncryptPlaceInQueue(expectedPlaceInqueue);
            long unixTimestamp = Hashing.GetTimestamp();

            string urlNoHash = "http://q.queue-it.net/inqueue.aspx?c=somecust&e=someevent&q=" + expectedQueueId +
                "&p=" + placeInQueueEncrypted + "&ts=" + unixTimestamp + "&h=";
            Uri hashUri = new Uri(urlNoHash);

            string hash = Hashing.GenerateMD5Hash(hashUri.AbsoluteUri, SharedSecreteEventKey);
            string querystring = "c=somecust&e=someevent&q=" + expectedQueueId +
                "&p=" + placeInQueueEncrypted + "&ts=" + unixTimestamp + "&h=" + hash;
            string url = urlNoHash + hash;

            HttpRequest httpRequest = new HttpRequest("inqueue.aspx", url, querystring);
            HttpContext.Current = new HttpContext(httpRequest, new HttpResponse(null));

            AcceptedConfirmedResult firstResult = SessionValidationController.ValidateRequest(
                QueueFactory.CreateQueue("somecust", "someevent")) as AcceptedConfirmedResult;


            Assert.IsNotNull(firstResult);
            Assert.AreEqual(true, firstResult.IsInitialValidationRequest);
            Assert.AreEqual(expectedQueueId, firstResult.KnownUser.QueueId);

            AcceptedConfirmedResult secondResult = SessionValidationController.ValidateRequest(
                QueueFactory.CreateQueue("somecust", "someevent")) as AcceptedConfirmedResult;

            Assert.IsNotNull(secondResult);
            Assert.IsFalse(secondResult.IsInitialValidationRequest);
        }

        [TestMethod]
        [ExpectedException(typeof(ExpiredValidationException))]
        public void SessionValidationController_ValidateRequest_KnownUserExpired_Test()
        {
            KnownUserFactory.Reset(false);
            KnownUserFactory.Configure(SharedSecreteEventKey);

            int expectedPlaceInqueue = 7810;
            Guid expectedQueueId = Guid.NewGuid();
            string placeInQueueEncrypted = Hashing.EncryptPlaceInQueue(expectedPlaceInqueue);
            long unixTimestamp =
                (long)(DateTime.UtcNow.AddMinutes(-4) - new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc)).TotalSeconds;

            string urlNoHash = "http://q.queue-it.net/inqueue.aspx?c=somecust&e=someevent&q=" + expectedQueueId +
                "&p=" + placeInQueueEncrypted + "&ts=" + unixTimestamp + "&h=";
            Uri hashUri = new Uri(urlNoHash);

            string hash = Hashing.GenerateMD5Hash(hashUri.AbsoluteUri, SharedSecreteEventKey);
            string querystring = "c=somecust&e=someevent&q=" + expectedQueueId +
                "&p=" + placeInQueueEncrypted + "&ts=" + unixTimestamp + "&h=" + hash;
            string url = urlNoHash + hash;

            HttpRequest httpRequest = new HttpRequest("inqueue.aspx", url, querystring);
            HttpContext.Current = new HttpContext(httpRequest, new HttpResponse(null));

            SessionValidationController.ValidateRequest(
                QueueFactory.CreateQueue("somecust", "someevent"));
        }

        [TestMethod]
        [ExpectedException(typeof(KnownUserValidationException))]
        public void SessionValidationController_ValidateRequest_InvalidKnownUserHash_Test()
        {
            KnownUserFactory.Reset(false);
            KnownUserFactory.Configure(SharedSecreteEventKey);

            int expectedPlaceInqueue = 7810;
            Guid expectedQueueId = Guid.NewGuid();
            string placeInQueueEncrypted = Hashing.EncryptPlaceInQueue(expectedPlaceInqueue);
            long unixTimestamp =
                (long)(DateTime.UtcNow.AddMinutes(-4) - new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc)).TotalSeconds;

            string urlNoHash = "http://q.queue-it.net/inqueue.aspx?c=somecust&e=someevent&q=" + expectedQueueId +
                "&p=" + placeInQueueEncrypted + "&ts=" + unixTimestamp + "&h=";
            Uri hashUri = new Uri(urlNoHash);

            string querystring = "c=somecust&e=someevent&q=" + expectedQueueId +
                "&p=" + placeInQueueEncrypted + "&ts=" + unixTimestamp + "&h=lksahjfdkerdbvlaewrsadf";
            string url = urlNoHash + "lksahjfdkerdbvlaewrsadf";

            HttpRequest httpRequest = new HttpRequest("inqueue.aspx", url, querystring);
            HttpContext.Current = new HttpContext(httpRequest, new HttpResponse(null));

            SessionValidationController.ValidateRequest(
                QueueFactory.CreateQueue("somecust", "someevent"));
        }

    }

    public class MockValidationResultRepository : IValidateResultRepository
    {
        private Dictionary<string, IValidateResult> _results = new Dictionary<string, IValidateResult>();

        public IValidateResult GetValidationResult(IQueue queue)
        {
            if (this._results.ContainsKey(queue.CustomerId + queue.EventId))
                return this._results[queue.CustomerId + queue.EventId];

            return null;
        }

        public void SetValidationResult(IQueue queue, IValidateResult validationResult, DateTime? expirationTime = null)
        {
            this._results.Add(queue.CustomerId + queue.EventId, validationResult);
        }

        public void Cancel(IQueue queue, IValidateResult validationResult)
        {
            throw new NotImplementedException();
        }
    }
}
