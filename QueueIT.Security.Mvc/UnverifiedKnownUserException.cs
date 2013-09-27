namespace QueueIT.Security.Mvc
{
    /// <summary>
    /// Thrown if the Known User request does not contaion any tokens
    /// </summary>
    public class UnverifiedKnownUserException : KnownUserException
    {

        public UnverifiedKnownUserException()
            : base("Known User tokens are missing", null)
        { }
    }
}