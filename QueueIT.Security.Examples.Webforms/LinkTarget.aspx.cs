using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;

namespace QueueIT.Security.Examples.Webforms
{
    public partial class LinkTarget : System.Web.UI.Page
    {
        protected override void OnPreInit(EventArgs e)
        {
            try
            {
                IKnownUser knownUser = KnownUserFactory.VerifyMd5Hash();

                if (knownUser == null)
                    Response.Redirect("Link.aspx");

                if (knownUser.TimeStamp < DateTime.UtcNow.Subtract(TimeSpan.FromMinutes(3)))
                    Response.Redirect("Link.aspx");

                PersistModel model = new PersistModel(
                    knownUser.QueueId,
                    knownUser.PlaceInQueue,
                    knownUser.TimeStamp);

                model.Persist();
            }
            catch (KnownUserException ex)
            {
                UriBuilder targetUrl = new UriBuilder(Request.Url);
                targetUrl.Path = "Link.aspx";

                Response.Redirect("Error.aspx?queuename=link&t=" + HttpUtility.UrlEncode(targetUrl.Uri.AbsoluteUri));
            }

            base.OnPreInit(e);
        }
        protected void Page_Load(object sender, EventArgs e)
        {

        }
    }
}