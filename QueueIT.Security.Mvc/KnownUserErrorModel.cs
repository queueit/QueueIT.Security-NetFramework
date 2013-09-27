using System;

namespace QueueIT.Security.Mvc
{
    /// <summary>
    /// View model which is sent to the known user error view on request validation exceptions
    /// </summary>
    /// <example>
    /// <code language="cs">
    /// public class AdvancedController : Controller
    /// {
    ///     [KnownUser(ErrorView = "MyErrorView")]
    ///     public ActionResult Index()
    ///     {
    ///         return View();
    ///     }
    /// }
    /// </code>
    /// <code>
    /// <![CDATA[
    /// @model QueueIT.Security.Mvc.KnownUserErrorModel
    /// 
    /// @{
    ///     ViewBag.Title = "Validation Error";
    ///     Layout = "~/Views/Shared/_Layout.cshtml";
    /// }
    /// 
    /// <div>An error occured.</div>
    /// <div>
    ///     <a href="/">Back To Home</a>
    /// </div>
    /// ]]>
    /// </code>
    /// </example>
    public class KnownUserErrorModel
    {
        /// <summary>
        /// The known user exception thrown
        /// </summary>
        public KnownUserException Exception { get; private set; }

        internal KnownUserErrorModel(KnownUserException exception)
        {
            Exception = exception;
        }
    }
}
