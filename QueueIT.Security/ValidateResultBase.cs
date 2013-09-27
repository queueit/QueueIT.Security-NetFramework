namespace QueueIT.Security
{
    /// <summary>
    /// Base class for validation results
    /// </summary>
    /// <seealso cref="AcceptedConfirmedResult" />
    /// <seealso cref="EnqueueResult" />
    public abstract class ValidateResultBase : IValidateResult
    {
        /// <summary>
        /// The queue of the request validation
        /// </summary>
        public IQueue Queue { get; private set; }

        internal ValidateResultBase(IQueue queue)
        {
            this.Queue = queue;
        }
    }
}