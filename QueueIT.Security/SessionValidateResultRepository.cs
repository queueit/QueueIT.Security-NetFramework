using System.Web;

namespace QueueIT.Security
{
    internal class SessionValidateResultRepository : ValidateResultRepositoryBase
    {
        public override IValidateResult GetValidationResult(IQueue queue)
        {
            var key = GenerateKey(queue.CustomerId, queue.EventId);
            return HttpContext.Current.Session[key] as IValidateResult;
        }

        public override void SetValidationResult(IQueue queue, IValidateResult validationResult)
        {
            var key = GenerateKey(queue.CustomerId, queue.EventId);
            HttpContext.Current.Session[key] = validationResult;
        }
    }
}
