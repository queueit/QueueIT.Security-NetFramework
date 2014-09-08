using System;
using System.Web;
using System.Web.UI;

namespace QueueIT.Security.Examples.Webforms
{
    public partial class Simple : Page
    {
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
                IValidateResult result = SessionValidationController.ValidateRequest();

                // Check if user must be enqueued
                if (result is EnqueueResult)
                {
                    Response.Redirect((result as EnqueueResult).RedirectUrl.AbsoluteUri);
                }
            }
            catch (ExpiredValidationException ex)
            {
                // Known user has has expired - Show error page and use GetCancelUrl to get user back in the queue
                Response.Redirect("Error.aspx?queuename=default&t=" + HttpUtility.UrlEncode(ex.KnownUser.OriginalUrl.AbsoluteUri));
            }
            catch (KnownUserValidationException ex)
            {
                // Known user is invalid - Show error page and use GetCancelUrl to get user back in the queue
                Response.Redirect(
                    "Error.aspx?queuename=default&t=" + 
                    HttpUtility.UrlEncode((ex.InnerException as KnownUserException).OriginalUrl.AbsoluteUri));
            }
        }

        protected void Page_Load(object sender, EventArgs e)
        { }
    }
}