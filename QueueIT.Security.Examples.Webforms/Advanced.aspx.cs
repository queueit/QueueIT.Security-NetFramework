using System;
using System.Net;
using System.Web;
using System.Web.UI;

namespace QueueIT.Security.Examples.Webforms
{
    public partial class Advanced : Page
    {
        static DateTime _lastCheck = DateTime.MinValue;
        static bool _healthCheckStatus = false;

        protected void Page_PreInit(object sender, EventArgs e)
        {
            QueueITValidation();
        }

        /// <summary>
        /// Queue validation
        /// </summary>
        /// <remarks>
        /// Please be aware that this this implementation is not done on error handling pages (e.g. Error.aspx) which will cause users to get looped arround.
        /// </remarks>
        private void QueueITValidation()
        {
                try
                {
                    IValidateResult result = SessionValidationController.ValidateRequest("advanced");

                    // Check if user must be enqueued (new users)
                    if (result is EnqueueResult)
                    {
                        if (QueueIsHealthy(result.Queue)) //Is Queue-it service online for my queue?
                            Response.Redirect((result as EnqueueResult).RedirectUrl.AbsoluteUri);
                    }

                    // This part checks if user has been through the queue and persists the users queue details for later tracking
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

                        model.Persist(); //Persist users queue details
                    }
                }
                catch (ExpiredValidationException ex)
                {
                    // Known user has has expired - Show error page and use GetCancelUrl to get user back in the queue
                    Response.Redirect("Error.aspx?queuename=advanced&t=" + HttpUtility.UrlEncode(ex.KnownUser.OriginalUrl.AbsoluteUri));
                }
                catch (KnownUserValidationException ex)
                {
                    // The known user url or hash is not valid - Show error page and use GetCancelUrl to get user back in the queue
                    Response.Redirect("Error.aspx?queuename=advanced&t=" + HttpUtility.UrlEncode((ex.InnerException as KnownUserException).OriginalUrl.AbsoluteUri));
                }
        }

        private bool QueueIsHealthy(IQueue Queue)
        {
            if (_lastCheck < DateTime.Now.Subtract(new TimeSpan(0, 0, 30))) //only chekc health every 30 seconds
            {
                try
                {
                    HttpWebRequest request = (HttpWebRequest)WebRequest.Create(
                        string.Format("https://healthcheck.queue-it.net/healthcheck.aspx/?c={0}&e={1}", Queue.CustomerId, Queue.EventId)); 
                    request.Timeout = 2000; // time out after 2 seconds
                    HttpWebResponse response = (HttpWebResponse)request.GetResponse();
                    _healthCheckStatus = response.StatusCode == HttpStatusCode.OK;
                }
                catch (Exception ex)
                {
                    _healthCheckStatus = false;
                }
                _lastCheck = DateTime.Now;
            }
            return _healthCheckStatus;
        }
    }
}