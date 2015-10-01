using System;
using QueueIT.Security.Configuration;

namespace QueueIT.Security
{
    public abstract class ValidateResultRepositoryBase : IValidateResultRepository
    {
        protected static bool ExtendValidity = true;
        protected static TimeSpan IdleExpiration = TimeSpan.FromMinutes(3);
        private const string SessionQueueId = "QueueITAccepted-SDFrts345E-";

        protected static string GenerateKey(string customerId, string eventId)
        {
            return string.Concat(SessionQueueId, customerId.ToLower(), "-", eventId.ToLower());
        }

        protected static void LoadBaseConfiguration()
        {
            SettingsSection settings = SettingsSection.GetSection();
            if (settings != null && settings.RepositorySettings != null)
            {
                SetTimespanFromRepositorySettings(
                    settings.RepositorySettings, "IdleExpiration", (value) => IdleExpiration = value);
            }
            if (settings != null && settings.RepositorySettings != null)
            {
                SetBooleanFromRepositorySettings(
                    settings.RepositorySettings, "ExtendValidity", (value) => ExtendValidity = value);
            }
        }

        protected static void ConfigureBase(
            TimeSpan idleExpiration = default(TimeSpan),
            bool extendValidity = true)
        {
            if (idleExpiration != default(TimeSpan))
                IdleExpiration = idleExpiration;
            if (extendValidity != true)
                ExtendValidity = extendValidity;
        }

        protected static void ClearBase()
        {
            IdleExpiration = TimeSpan.FromMinutes(3);
            ExtendValidity = true;
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

        protected static void SetBooleanFromRepositorySettings(
            RepositorySettingsCollection settings, string cookieName, Action<bool> updateValue)
        {
            string cookieNameString = GetValue(cookieName, settings);
            bool boolean;
            if (Boolean.TryParse(cookieNameString, out boolean))
                updateValue.Invoke(boolean);
        }
    }
}