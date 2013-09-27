using System;
using System.Security;

namespace QueueIT.Security
{
    /// <summary>
    /// Thrown if the session validation fails
    /// </summary>
    public class SessionValidationException : SecurityException
    {
        /// <summary>
        /// The queue of the Known User request
        /// </summary>
        public IQueue Queue { get; private set; }

        internal SessionValidationException(string message, IQueue queue)
            : base(message)
        {
            this.Queue = queue;
        }

        internal SessionValidationException(string message, Exception innerException, IQueue queue)
            : base(message, innerException)
        {
            this.Queue = queue;
        }
    }
}