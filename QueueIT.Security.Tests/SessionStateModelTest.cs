using System;
using System.IO;
using System.Xml.Serialization;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace QueueIT.Security.Tests
{
    [TestClass]
    public class SessionStateModelTest
    {
        [TestMethod]
        public void SessionStateModel_Serialize_Test()
        {
            XmlSerializer s = new XmlSerializer(typeof(SessionStateModel));

            SessionStateModel expectedModel = new SessionStateModel()
            {
                OriginalUri = "http://test.com/abc",
                PlaceInQueue = 14575,
                QueueId = Guid.NewGuid(),
                RedirectType = RedirectType.Queue,
                TimeStamp = DateTime.UtcNow,
                Expiration = DateTime.UtcNow.AddMinutes(3)
            };

            MemoryStream stream = new MemoryStream();
            s.Serialize(stream, expectedModel);

            stream.Position = 0;

            SessionStateModel actualModel = s.Deserialize(stream) as SessionStateModel;

            Assert.IsNotNull(actualModel);
            Assert.AreEqual(expectedModel.OriginalUri, actualModel.OriginalUri);
            Assert.AreEqual(expectedModel.PlaceInQueue, actualModel.PlaceInQueue);
            Assert.AreEqual(expectedModel.QueueId, actualModel.QueueId);
            Assert.AreEqual(expectedModel.RedirectType, actualModel.RedirectType);
            Assert.AreEqual(expectedModel.TimeStamp, actualModel.TimeStamp);
            Assert.AreEqual(expectedModel.Expiration, actualModel.Expiration);

        }
    }
}
