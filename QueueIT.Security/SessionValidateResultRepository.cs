using System.Web;

namespace QueueIT.Security
{
    internal class SessionValidateResultRepository : IValidateResultRepository
    {
        private const string SessionQueueId = "QueueITAccepted-SDFrts345E-";

        public IValidateResult GetValidationResult(IQueue queue)
        {
            var key = GenerateKey(queue.CustomerId, queue.EventId);
            return HttpContext.Current.Session[key] as IValidateResult;
        }

        public void SetValidationResult(IQueue queue, IValidateResult validationResult)
        {
            var key = GenerateKey(queue.CustomerId, queue.EventId);
            HttpContext.Current.Session[key] = validationResult;
        }

        private static string GenerateKey(string customerId, string eventId)
        {
            return string.Concat(SessionQueueId, customerId, "-", eventId);
        }
    }
}
