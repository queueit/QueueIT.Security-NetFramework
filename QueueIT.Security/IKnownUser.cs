using System;

namespace QueueIT.Security
{
    /// <summary>
    /// A reprecentation of the details of a known user queue id request
    /// </summary>
    public interface IKnownUser
    {
        /// <summary>
        /// The Queue ID of the request
        /// </summary>
        Guid QueueId { get; }
        /// <summary>
        /// The Queue Number of the Queue ID. 
        /// This may be null if the queue number is unknown at the time of redirect or the request is a safetynet redirect
        /// </summary>
        int? PlaceInQueue { get; }
        /// <summary>
        /// The UTC timestamp of when the request was initialized
        /// </summary>
        DateTime TimeStamp { get; }
        /// <summary>
        /// The Customer ID of the Queue ID
        /// </summary>
        string CustomerId { get; }
        /// <summary>
        /// The Event ID of the Queue ID
        /// </summary>
        string EventId { get; }
        /// <summary>
        /// The URL the user was redirected to without known user parameters
        /// </summary>
        string OriginalUrl { get; }
        /// <summary>
        /// The type of redirect
        /// </summary>
        RedirectType RedirectType { get; }
    }
}