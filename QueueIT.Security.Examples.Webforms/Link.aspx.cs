using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;

namespace QueueIT.Security.Examples.Webforms
{
    public partial class Link : System.Web.UI.Page
    {
        protected void Page_Load(object sender, EventArgs e)
        {
            UriBuilder targetUrl = new UriBuilder(Request.Url);
            targetUrl.Path = "LinkTarget.aspx";

            IQueue queue = QueueFactory.CreateQueue("link");

            hlQueue.NavigateUrl = queue.GetQueueUrl(targetUrl.Uri).AbsoluteUri;
        }
    }
}