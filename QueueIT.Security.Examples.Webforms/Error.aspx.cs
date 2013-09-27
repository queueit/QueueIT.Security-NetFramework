using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;

namespace QueueIT.Security.Examples.Webforms
{
    public partial class Error : System.Web.UI.Page
    {
        protected void Page_Load(object sender, EventArgs e)
        {
            string queueName = Request.QueryString["queuename"];

            IQueue queue = QueueFactory.CreateQueue(queueName);

            hlQueue.NavigateUrl = queue.GetCancelUrl(new Uri(Request.QueryString["t"])).AbsoluteUri;
        }
    }
}