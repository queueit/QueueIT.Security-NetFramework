namespace QueueIT.Security
{
    /// <summary>
    /// Validation result when it is confirmed that the user has been through the queue
    /// </summary>
    public class AcceptedConfirmedResult : ValidateResultBase
    {
        /// <summary>
        /// Known User details of the user
        /// </summary>
        public IKnownUser KnownUser { get; private set; }

        /// <summary>
        /// If true this current request has validated the user. Otherwise it is a cached result.
        /// </summary>
        public bool IsInitialValidationRequest { get; private set; }

        internal AcceptedConfirmedResult(IQueue queue, IKnownUser knownUser, bool initialRequest)
            : base(queue)
        {
            this.KnownUser = knownUser;
            this.IsInitialValidationRequest = initialRequest;
        }
    }
}