using System;

namespace QueueIT.Security
{
    /// <summary>
    /// A repository to store user session state. This can be implemented if ASP.NET Sessions is unavailable. 
    /// </summary>
    public interface IValidateResultRepository
    {
        /// <summary>
        /// Gets the validation result of a user from the session
        /// </summary>
        /// <param name="queue">The queue of the validation result</param>
        /// <returns>The validation result of the user session if any. 
        /// Must return null if the users has not previously been validated.</returns>
        IValidateResult GetValidationResult(IQueue queue);

        /// <summary>
        /// Sets the validation result of a user on the session
        /// </summary>
        /// <param name="queue">The queue of the validation result</param>
        /// <param name="validationResult">The validation result of the user</param>
        /// <param name="expirationTime">The time where the result will expire</param>
        void SetValidationResult(IQueue queue, IValidateResult validationResult, DateTime? expirationTime = null);

        /// <summary>
        /// Cancels a validation result
        /// </summary>
        /// <param name="queue">The queue of the validation result</param>
        /// <param name="validationResult">The validation result of the user</param>
        void Cancel(IQueue queue, IValidateResult validationResult);
    }
}