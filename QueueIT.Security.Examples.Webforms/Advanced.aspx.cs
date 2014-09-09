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
        private IValidateResult _result;

        protected void Page_PreInit(object sender, EventArgs e)
        {
            QueueITValidation();
        }

        private void QueueITValidation()
        {
                try
                {
                    this._result = SessionValidationController.ValidateRequest("advanced");
                    var enqueue = this._result as EnqueueResult;

                    // Check if user must be enqueued (new users)
                    if (enqueue != null)
                    {
                        if (QueueIsHealthy(enqueue.Queue)) //Is Queue-it service online for my queue?
                            Response.Redirect(enqueue.RedirectUrl.AbsoluteUri);
                    }

                    // This part checks if user has been through the queue and persists the users queue details for later tracking
                    var accepted = this._result as AcceptedConfirmedResult;
                    if (accepted != null)
                    {
                        if (!accepted.IsInitialValidationRequest)
                            return; // data has already been persisted

                        PersistModel model = new PersistModel(
                            accepted.Queue.CustomerId,
                            accepted.Queue.EventId,
                            accepted.KnownUser.QueueId,
                            accepted.KnownUser.PlaceInQueue,
                            accepted.KnownUser.TimeStamp);

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

        protected void Page_Load(object sender, EventArgs e)
        {
            var accepted = this._result as AcceptedConfirmedResult;
            if (accepted != null)
            {
                var currentUrl = HttpContext.Current.Request.Url.AbsoluteUri.ToLower();
                hlCancel.NavigateUrl = accepted.Queue.GetCancelUrl(
                    new Uri(currentUrl.Substring(0, currentUrl.IndexOf("advanced.aspx")) + "cancel.aspx?eventid=advanced"),
                    accepted.KnownUser.QueueId).ToString();
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