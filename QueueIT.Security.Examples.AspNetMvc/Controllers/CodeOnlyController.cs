﻿using System.Web.Mvc;
using QueueIT.Security.Mvc;

namespace QueueIT.Security.Examples.AspNetMvc.Controllers
{
    public class CodeOnlyController : Controller
    {
        static CodeOnlyController()
        {
            KnownUserFactory.Configure("a774b1e2-8da7-4d51-b1a9-7647147bb13bace77210-a488-4b6f-afc9-8ba94551a7d7");
        }

        [SessionValidation("ticketania", "codeonly", IncludeTargetUrl = true)] // Please be aware that this filter is not applied to error controller actions or similar which will cause users to get looped arround.
        public ActionResult Index()
        {
            return View();
        }

    }
}
