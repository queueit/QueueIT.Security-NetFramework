using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace QueueIT.Security.Mvc
{
    /// <summary>
    /// View model for known user validation which is added to the action parameters list if included in the parameters of the controller action
    /// </summary>
    /// <example>
    /// <code language="cs">
    /// public class LinkController : Controller
    /// {
    ///     [KnownUser]
    ///     public ActionResult Landing(KnownUserModel knownUserModel)
    ///     {
    ///         Guid queueId = knownUserModel.KnownUser.QueueId;
    /// 
    ///         return View();
    ///     }
    /// }
    /// </code>
    /// </example>
    public class KnownUserModel
    {
        /// <summary>
        /// The validated Known User Object
        /// </summary>
        public IKnownUser KnownUser { get; internal set; }
    }
}
