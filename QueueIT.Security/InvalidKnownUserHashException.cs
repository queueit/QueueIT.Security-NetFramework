using System;
using System.Security;

namespace QueueIT.Security
{
    /// <summary>
    /// Thrown if the hash of the Known User request is invalid
    /// </summary>
    public class InvalidKnownUserHashException : KnownUserException
    {
        internal InvalidKnownUserHashException()
            : base("The hash of the request is invalid", null)
        {}
    }
}