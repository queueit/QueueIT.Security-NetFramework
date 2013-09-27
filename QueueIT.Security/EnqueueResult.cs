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
        public Uri RedirectUrl { get; private set; }

        internal EnqueueResult(IQueue queue, Uri redirectUrl)
            : base(queue)
        {
            RedirectUrl = redirectUrl;
        }
    }
}