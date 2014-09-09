using System;
using System.Web;
using System.Web.UI;

namespace QueueIT.Security.Examples.Webforms
{
    public partial class CodeOnly : Page
    {
        private IValidateResult _result;
        static CodeOnly()
        {
            // Configure the shared key (should be done once - e.g. in global.asax) 
            KnownUserFactory.Configure("a774b1e2-8da7-4d51-b1a9-7647147bb13bace77210-a488-4b6f-afc9-8ba94551a7d7");
        }

        protected void Page_PreInit(object sender, EventArgs e)
        {
            QueueITValidation();
        }

        private void QueueITValidation()
        {
            try
            {
                this._result = SessionValidationController.ValidateRequest("ticketania", "codeonly", includeTargetUrl: true);
                var enqueue = this._result as EnqueueResult;

                // Check if user must be enqueued
                if (enqueue != null)
                {
                    Response.Redirect(enqueue.RedirectUrl.AbsoluteUri);
                }
            }
            catch (ExpiredValidationException ex)
            {
                // Known user has has expired - Show error page and use GetCancelUrl to get user back in the queue
                Response.Redirect("Error.aspx?queuename=codeonly&t=" + HttpUtility.UrlEncode(ex.KnownUser.OriginalUrl.AbsoluteUri));
            }
            catch (KnownUserValidationException ex)
            {
                // Known user is invalid - Show error page and use GetCancelUrl to get user back in the queue
                Response.Redirect(
                    "Error.aspx?queuename=codeonly&t=" + 
                    HttpUtility.UrlEncode((ex.InnerException as KnownUserException).OriginalUrl.AbsoluteUri));
            }
        }
        protected void Page_Load(object sender, EventArgs e)
        {
            var accepted = this._result as AcceptedConfirmedResult;
            if (accepted != null)
            {
                var currentUrl = HttpContext.Current.Request.Url.AbsoluteUri.ToLower();
                hlCancel.NavigateUrl = accepted.Queue.GetCancelUrl(
                    new Uri(currentUrl.Substring(0, currentUrl.IndexOf("codeonly.aspx")) + "cancel.aspx?eventId=codeonly"),
                    accepted.KnownUser.QueueId).ToString();
            }
        }
    }
}