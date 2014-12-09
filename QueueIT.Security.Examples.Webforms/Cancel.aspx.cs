using System;

namespace QueueIT.Security.Examples.Webforms
{
    public partial class Cancel : System.Web.UI.Page
    {
        protected void Page_Load(object sender, EventArgs e)
        {
            IValidateResult result = SessionValidationController.ValidateRequest(
                QueueFactory.CreateQueue("ticketania", Request.QueryString["eventid"]));
            var accepted = result as AcceptedConfirmedResult;

            if (accepted != null)
            {
                accepted.Cancel();
            }
        }
    }
}