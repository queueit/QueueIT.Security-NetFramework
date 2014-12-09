using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace QueueIT.Security.Tests
{
    [TestClass]
    public class Md5KnownUserTest
    {
        [TestMethod]
        public void Md5KnownUser_Constructor_Test()
        {
            //Arrange
            Guid expectedQueueId = Guid.NewGuid();
            DateTime expectedTimeStamp = DateTime.UtcNow;
            int expectedPlaceInQueue = 465;
            string expectedCustomerId = "somecust";
            string expectedEventId = "someevent";
            string expectedOriginalUrl = "http://google.com/";
            RedirectType expectedRedirectType = RedirectType.Safetynet;

            //Act
            Md5KnownUser knownUser = new Md5KnownUser(
                expectedQueueId, 
                expectedPlaceInQueue, 
                expectedTimeStamp, 
                expectedCustomerId, 
                expectedEventId, 
                expectedRedirectType,
                expectedOriginalUrl);

            //Assert        
            Assert.AreEqual(expectedQueueId, knownUser.QueueId);
            Assert.IsTrue(knownUser.PlaceInQueue.HasValue);
            Assert.AreEqual(expectedPlaceInQueue, knownUser.PlaceInQueue.Value);
            Assert.AreEqual(expectedTimeStamp, knownUser.TimeStamp);
            Assert.AreEqual(expectedCustomerId, knownUser.CustomerId);
            Assert.AreEqual(expectedEventId, knownUser.EventId);
            Assert.AreEqual(expectedRedirectType, knownUser.RedirectType);
            Assert.AreEqual(expectedOriginalUrl, knownUser.OriginalUrl);
        }

        [TestMethod]
        public void Md5KnownUser_Constructor_PlaceInQueueNotKnown_Test()
        {
            //Arrange
            Guid expectedQueueId = Guid.NewGuid();
            DateTime expectedTimeStamp = DateTime.UtcNow;

            //Act
            Md5KnownUser knownUser = new Md5KnownUser(expectedQueueId, 9999999, expectedTimeStamp, null, null, RedirectType.Queue, null);

            //Assert        
            Assert.AreEqual(expectedQueueId, knownUser.QueueId);
            Assert.IsFalse(knownUser.PlaceInQueue.HasValue);
            Assert.AreEqual(expectedTimeStamp, knownUser.TimeStamp);
        }

    }
}
