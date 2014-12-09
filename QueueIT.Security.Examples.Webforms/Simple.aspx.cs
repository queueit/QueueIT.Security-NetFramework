using System;
using System.Web;
using System.Web.UI;

namespace QueueIT.Security.Examples.Webforms
{
    public partial class Simple : Page
    {
        private IValidateResult _result;
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
                this._result = SessionValidationController.ValidateRequest();
                var enqueue = this._result as EnqueueResult;

                // Check if user must be enqueued
                if (enqueue != null)
                {
                    Response.Redirect(enqueue.RedirectUrl);
                }
            }
            catch (ExpiredValidationException ex)
            {
                // Known user has has expired - Show error page and use GetCancelUrl to get user back in the queue
                Response.Redirect("Error.aspx?queuename=default&t=" + HttpUtility.UrlEncode(ex.KnownUser.OriginalUrl));
            }
            catch (KnownUserValidationException ex)
            {
                // Known user is invalid - Show error page and use GetCancelUrl to get user back in the queue
                Response.Redirect(
                    "Error.aspx?queuename=default&t=" + 
                    HttpUtility.UrlEncode((ex.InnerException as KnownUserException).OriginalUrl));
            }
        }

        protected void Page_Load(object sender, EventArgs e)
        {
            var accepted = this._result as AcceptedConfirmedResult;
            if (accepted != null)
            {
                var currentUrl = HttpContext.Current.Request.Url.AbsoluteUri.ToLower();
                hlCancel.NavigateUrl = accepted.Queue.GetCancelUrl(
                    currentUrl.Substring(0, currentUrl.IndexOf("simple.aspx")) + "cancel.aspx?eventid=simple",
                    accepted.KnownUser.QueueId).ToString();
                hlExpire.NavigateUrl = "Expire.aspx?eventid=simple";
            }

        }
    }
}