namespace QueueIT.Security
{
    public abstract class ValidateResultRepositoryBase : IValidateResultRepository
    {
        private const string SessionQueueId = "QueueITAccepted-SDFrts345E-";

        protected static string GenerateKey(string customerId, string eventId)
        {
            return string.Concat(SessionQueueId, customerId.ToLower(), "-", eventId.ToLower());
        }

        public abstract IValidateResult GetValidationResult(IQueue queue);
        public abstract void SetValidationResult(IQueue queue, IValidateResult validationResult);
    }
}