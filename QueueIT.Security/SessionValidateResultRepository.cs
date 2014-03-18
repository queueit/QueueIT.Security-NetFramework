using System;
using System.Web;

namespace QueueIT.Security
{
    /// <summary>
    /// A repository to store user session state which stores the validation result in a http cookie.
    /// </summary>
    /// <remarks>The default repository is the CookieValidateResultRepository. Use the Configure method of the SessionValidationController to override.</remarks>
    /// <example>
    /// <code language="cs">
    /// SessionValidationController.Configure(validationResultProviderFactory: () => new SessionValidateResultRepository);
    /// </code>
    /// </example>
    public class SessionValidateResultRepository : ValidateResultRepositoryBase
    {
        public override IValidateResult GetValidationResult(IQueue queue)
        {
            var key = GenerateKey(queue.CustomerId, queue.EventId);
            SessionStateModel model = HttpContext.Current.Session[key] as SessionStateModel;

            if (model == null)
                return null;

            return new AcceptedConfirmedResult(
                queue, 
                new Md5KnownUser(
                    model.QueueId,
                    model.PlaceInQueue,
                    model.TimeStamp,
                    queue.CustomerId,
                    queue.EventId,
                    model.RedirectType,
                    new Uri(model.OriginalUri)), 
                false);
        }

        public override void SetValidationResult(IQueue queue, IValidateResult validationResult)
        {
            AcceptedConfirmedResult acceptedResult = validationResult as AcceptedConfirmedResult;

            if (acceptedResult != null)
            {

                var key = GenerateKey(queue.CustomerId, queue.EventId);
                SessionStateModel model = new SessionStateModel()
                {
                    QueueId = acceptedResult.KnownUser.QueueId,
                    OriginalUri = acceptedResult.KnownUser.OriginalUrl.AbsoluteUri,
                    PlaceInQueue = acceptedResult.KnownUser.PlaceInQueue,
                    TimeStamp = acceptedResult.KnownUser.TimeStamp,
                    RedirectType = acceptedResult.KnownUser.RedirectType
                };
                HttpContext.Current.Session[key] = model;
            }
        }
    }
}
