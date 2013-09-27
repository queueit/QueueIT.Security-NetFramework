using System.Security;

namespace QueueIT.Security
{
    /// <summary>
    /// Thrown if the Known User request does not contaion the required parameters
    /// </summary>
    public class InvalidKnownUserUrlException : KnownUserException
    {
        
        internal InvalidKnownUserUrlException()
            : base("The url of the request is invalid", null)
        {}
    }
}