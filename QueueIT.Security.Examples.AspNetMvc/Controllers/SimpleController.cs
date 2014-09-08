using System.Web.Mvc;
using QueueIT.Security.Mvc;

namespace QueueIT.Security.Examples.AspNetMvc.Controllers
{
    public class SimpleController : Controller
    {
        [SessionValidation] // // Please be aware that this filter is not applied to error controller actions or similar which will cause users to get looped arround.
        public ActionResult Index()
        {
            return View();
        }
    }
}
