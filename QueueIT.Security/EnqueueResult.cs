using System;

namespace QueueIT.Security
{
    /// <summary>
    /// Validation result when the user should be enqueued at Queue-it 
    /// </summary>
    public class EnqueueResult : ValidateResultBase
    {
        /// <summary>
        /// URL to redirect user to
        /// </summary>
        public string RedirectUrl { get; private set; }

        internal EnqueueResult(IQueue queue, string redirectUrl)
            : base(queue)
        {
            RedirectUrl = redirectUrl;
        }
    }
}