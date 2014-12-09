using System;
using System.Web.Mvc;
using QueueIT.Security.Examples.AspNetMvc.Filters;
using QueueIT.Security.Mvc;

namespace QueueIT.Security.Examples.AspNetMvc.Controllers
{
    public class AdvancedController : Controller
    {
        [CustomSessionValidation("advanced")] // Please be aware that this filter is not applied to error controller actions or similar which will cause users to get looped arround.
        public ActionResult Index(SessionValidationModel sessionValidationModel)
        {
            return View();
        }

        public ActionResult Landing(string t)
        {
            IQueue queue = QueueFactory.CreateQueue("advanced");

            ViewBag.QueueUrl = queue.GetQueueUrl(t);

            return View();
        }
    }
}
