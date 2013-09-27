namespace QueueIT.Security
{
    /// <summary>
    /// Represents a request validation result
    /// </summary>
    /// <seealso cref="QueueIT.Security.AcceptedConfirmedResult" />
    /// <seealso cref="EnqueueResult" />
    public interface IValidateResult
    {
        /// <summary>
        /// The queue of the validation request
        /// </summary>
        IQueue Queue { get; }
    }
}