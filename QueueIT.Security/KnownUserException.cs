using System;
using System.Security;

namespace QueueIT.Security
{
    /// <summary>
    /// Exception thrown if the Known User validation failed
    /// </summary>
    public abstract class KnownUserException : SecurityException
    {
        /// <summary>
        /// The URL the user was redirected to without known user parameters
        /// </summary>
        public Uri OriginalUrl { get; internal set; }

        protected KnownUserException(string message, Exception innerException)
            : base(message, innerException)
        {}
    }
}