using System;

namespace QueueIT.Security
{
    internal abstract class KnownUserBase : IKnownUser
    {
        private int? _placeInQueue;

        public Guid QueueId { get; protected set; }
        public int? PlaceInQueue
        {
            get
            {
                return this._placeInQueue;
            }
            protected set
            {
                this._placeInQueue = (value <= 0 || value >= 9999999) ? null : value;
            }
        }
        public DateTime TimeStamp { get; protected set; }
        public string CustomerId { get; protected set; }
        public string EventId { get; protected set; }
        public RedirectType RedirectType { get; protected set; }
        public Uri OriginalUrl { get; protected set; }
    }
}