using System;

namespace QueueIT.Security
{
    internal class Md5KnownUser : KnownUserBase
    {
        internal Md5KnownUser(
            Guid queueId, 
            int? placeInQueue, 
            DateTime timeStamp, 
            string customerId, 
            string eventId, 
            RedirectType redirectType, 
            Uri originalUrl)
        {
            this.QueueId = queueId;
            this.PlaceInQueue = placeInQueue;
            this.TimeStamp = timeStamp;
            this.CustomerId = customerId;
            this.EventId = eventId;
            this.OriginalUrl = originalUrl;
            this.RedirectType = redirectType;
        }
    }
}
