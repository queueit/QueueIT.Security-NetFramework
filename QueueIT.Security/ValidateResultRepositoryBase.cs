using System;
using QueueIT.Security.Configuration;

namespace QueueIT.Security
{
    public abstract class ValidateResultRepositoryBase : IValidateResultRepository
    {
        protected static TimeSpan IdleExpiration = TimeSpan.FromMinutes(3);
        protected static TimeSpan DisabledExpiration = TimeSpan.FromMinutes(3);
        private const string SessionQueueId = "QueueITAccepted-SDFrts345E-";

        protected static string GenerateKey(string customerId, string eventId)
        {
            return string.Concat(SessionQueueId, customerId.ToLower(), "-", eventId.ToLower());
        }

        public abstract IValidateResult GetValidationResult(IQueue queue);
        public abstract void SetValidationResult(IQueue queue, IValidateResult validationResult, DateTime? expirationTime = null);
        public abstract void Cancel(IQueue queue, IValidateResult validationResult);

        protected static string GetValue(string name, RepositorySettingsCollection repositorySettings)
        {
            foreach (RepositorySettingSection repositorySetting in repositorySettings)
            {
                if (repositorySetting.Name == name)
                    return repositorySetting.Value;
            }

            return null;
        }

        protected static void SetTimespanFromRepositorySettings(
            RepositorySettingsCollection settings, string cookieName, Action<TimeSpan> updateValue)
        {
            string cookieNameString = GetValue(cookieName, settings);
            TimeSpan timeSpan;
            if (TimeSpan.TryParse(cookieNameString, out timeSpan))
                updateValue.Invoke(timeSpan);
        }
    }
}