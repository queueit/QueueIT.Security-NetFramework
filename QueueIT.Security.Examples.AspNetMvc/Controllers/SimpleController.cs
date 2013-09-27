using System.Web.Mvc;
using QueueIT.Security.Mvc;

namespace QueueIT.Security.Examples.AspNetMvc.Controllers
{
    public class SimpleController : Controller
    {
        [SessionValidation]
        public ActionResult Index()
        {
            return View();
        }

    }
}
