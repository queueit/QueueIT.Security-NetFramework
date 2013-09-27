using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace QueueIT.Security.Mvc
{
    /// <summary>
    /// View model for request validations which is added to the action parameters list if included in the parameters of the controller action
    /// </summary>
    /// <example>
    /// <code language="cs">
    /// public class AdvancedController : Controller
    /// {
    ///     [SessionValidation("advanced")]
    ///     public ActionResult Index(SessionValidationModel validationModel)
    ///     {
    ///         IValidateResult result = validationModel.ValidateResult;
    /// 
    ///         return View();
    ///     }
    /// }
    /// </code>
    /// </example>
    public class SessionValidationModel
    {
        /// <summary>
        /// Validation result
        /// </summary>
        public IValidateResult ValidateResult { get; internal set; }

        /// <summary>
        /// Public constructor used by ASP.NET MVC
        /// </summary>
        public SessionValidationModel()
        {
        }
    }
}
