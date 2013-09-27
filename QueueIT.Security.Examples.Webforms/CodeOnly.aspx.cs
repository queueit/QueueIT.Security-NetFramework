using System;
using System.Web;
using System.Web.UI;

namespace QueueIT.Security.Examples.Webforms
{
    public partial class CodeOnly : Page
    {
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
                IValidateResult result = SessionValidationController.ValidateRequest("ticketania", "codeonly", includeTargetUrl: true);

                // Check if user must be enqueued
                if (result is EnqueueResult)
                {
                    Response.Redirect((result as EnqueueResult).RedirectUrl.AbsoluteUri);
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
        }
    }
}