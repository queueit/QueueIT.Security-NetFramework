using System;

namespace QueueIT.Security.Mvc
{
    /// <summary>
    /// View model which is sent to the validation error view on request validation exceptions
    /// </summary>
    /// <example>
    /// <code language="cs">
    /// public class AdvancedController : Controller
    /// {
    ///     [SessionValidation("advanced", ErrorView = "MyErrorView")]
    ///     public ActionResult Index()
    ///     {
    ///         return View();
    ///     }
    /// }
    /// </code>
    /// <code>
    /// <![CDATA[
    /// @model QueueIT.Security.Mvc.SessionValidationErrorModel
    /// 
    /// @{
    ///     ViewBag.Title = "Validation Error";
    ///     Layout = "~/Views/Shared/_Layout.cshtml";
    /// }
    /// 
    /// <div>An error occured.</div>
    /// <div>
    ///     <a href="/">Back To Home</a> <a href="@Model.Queue.GetCancelUrl(Model.OriginalUrl)">Go to queue</a>
    /// </div>
    /// ]]>
    /// </code>
    /// </example>
    public class SessionValidationErrorModel
    {
        /// <summary>
        /// The validation exception thrown
        /// </summary>
        public SessionValidationException Exception { get; private set; }
        /// <summary>
        /// The URL of the validation request without Known User details
        /// </summary>
        public string OriginalUrl { get; private set; }
        /// <summary>
        /// The queue of the validation request.
        /// </summary>
        public IQueue Queue { get; private set; }

        internal SessionValidationErrorModel(IQueue queue, SessionValidationException exception, string originalUrl)
        {
            Queue = queue;
            Exception = exception;
            OriginalUrl = originalUrl;
        }
    }
}
