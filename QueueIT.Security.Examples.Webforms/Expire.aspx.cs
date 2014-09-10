using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;

namespace QueueIT.Security.Examples.Webforms
{
    public partial class Expire : System.Web.UI.Page
    {
        protected void Page_Load(object sender, EventArgs e)
        {
            IValidateResult result = SessionValidationController.ValidateRequest("ticketania", Request.QueryString["eventid"]);
            var accepted = result as AcceptedConfirmedResult;

            if (accepted != null)
            {
                accepted.SetExpiration(DateTime.UtcNow.AddSeconds(15));
            }
        }
    }
}