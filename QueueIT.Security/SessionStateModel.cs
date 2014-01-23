using System;

namespace QueueIT.Security
{
    [Serializable]
    public class SessionStateModel
    {
        public Guid QueueId { get; set; }
        public string OriginalUri { get; set; }
        public DateTime TimeStamp { get; set; }
        public RedirectType RedirectType { get; set; }
        public int? PlaceInQueue { get; set; }
    }
}