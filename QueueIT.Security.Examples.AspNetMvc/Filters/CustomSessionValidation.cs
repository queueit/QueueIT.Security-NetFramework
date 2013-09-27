using System.Web.Mvc;
using QueueIT.Security.Examples.AspNetMvc.Models;
using QueueIT.Security.Mvc;

namespace QueueIT.Security.Examples.AspNetMvc.Filters
{
    public class CustomSessionValidation : SessionValidationAttribute
    {
        public CustomSessionValidation(string queueName)
            : base(queueName)
        {}

        protected override void OnValidated(ActionExecutingContext filterContext, IValidateResult result)
        {
            // Check if user must be enqueued
            if (result is EnqueueResult)
            {
                // Optional action
            }

            // Check if user has been through the queue (will be invoked for every page request after the user has been validated)
            if (result is AcceptedConfirmedResult)
            {
                AcceptedConfirmedResult confirmedResult = result as AcceptedConfirmedResult;

                if (!confirmedResult.IsInitialValidationRequest)
                    return; // data has already been persisted

                PersistModel model = new PersistModel(
                    confirmedResult.Queue.CustomerId,
                    confirmedResult.Queue.EventId,
                    confirmedResult.KnownUser.QueueId,
                    confirmedResult.KnownUser.PlaceInQueue,
                    confirmedResult.KnownUser.TimeStamp);

                model.Persist();
            }

            base.OnValidated(filterContext, result);
        }
    }
}