namespace QueueIT.Security
{
    /// <summary>
    /// Exception thrown if the Known User validation failed
    /// </summary>
    public class KnownUserValidationException : SessionValidationException
    {
        internal KnownUserValidationException(KnownUserException innerException, IQueue queue)
            : base(innerException.Message, innerException, queue)
        {
        }
    }
}