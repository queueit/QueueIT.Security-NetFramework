using System;

namespace QueueIT.Security.Examples.Webforms
{
    internal class PersistModel
    {
        public PersistModel(string customerId, string eventId, Guid queueId, int? placeInQueue, DateTime timeStamp)
        {
            
        }

        public PersistModel(Guid queueId, int? placeInQueue, DateTime timeStamp)
        {

        }

        public void Persist()
        {
        }
    }
}