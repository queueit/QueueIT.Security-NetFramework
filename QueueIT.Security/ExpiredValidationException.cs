using System.Security;

namespace QueueIT.Security
{
    /// <summary>
    /// Thrown if the Known User request URL is expired
    /// </summary>
    public class ExpiredValidationException  : SessionValidationException
    {
        /// <summary>
        /// The expired known User request
        /// </summary>
        public IKnownUser KnownUser { get; set; }

        internal ExpiredValidationException(IQueue queue, IKnownUser knownUser)
            : base("Known User token is expired", queue)
        {
            KnownUser = knownUser;
        }
    }
}