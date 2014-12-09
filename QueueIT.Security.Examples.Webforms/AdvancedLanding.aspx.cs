using System;

namespace QueueIT.Security.Examples.Webforms
{
    public partial class AdvancedLanding : System.Web.UI.Page
    {
        protected void Page_Load(object sender, EventArgs e)
        {
            IQueue queue = QueueFactory.CreateQueue("advanced");

            hlQueue.NavigateUrl = queue.GetQueueUrl(Request.QueryString["t"]);
        }
    }
}