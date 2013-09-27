using System;
using System.Web.Mvc;

namespace QueueIT.Security.Mvc
{
    /// <summary>
    /// Action Filter which enables Known User validation functionality
    /// </summary>
    /// <example>
    /// Source Code;
    /// <code>
    /// [KnownUser]
    /// public ActionResult Target(KnownUserModel knownUserModel)
    /// {
    ///     PersistModel model = new PersistModel(
    ///         knownUserModel.KnownUser.QueueId,
    ///         knownUserModel.KnownUser.PlaceInQueue,
    ///         knownUserModel.KnownUser.TimeStamp);
    ///
    ///     model.Persist();
    ///
    ///     return View();
    /// }
    /// </code>
    /// </example>    
    public class KnownUserAttribute : ActionFilterAttribute 
    {
        /// <summary>
        /// The view to display in case of a Known User validation exception. Default view is QueueITKnownUserError.
        /// </summary>
        public string ErrorView { get; set; }

        public KnownUserAttribute()
        {
            this.ErrorView = "QueueITKnownUserError";
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="filterContext">The Action Executing Filter Context</param>
        public sealed override void OnActionExecuting(ActionExecutingContext filterContext)
        {
            try
            {
                IKnownUser knownUser = KnownUserFactory.VerifyMd5Hash();

                if (knownUser == null)
                    throw new UnverifiedKnownUserException();

                foreach (var value in filterContext.ActionParameters.Values)
                {
                    if (value is KnownUserModel)
                        (value as KnownUserModel).KnownUser = knownUser;
                }
            }
            catch (KnownUserException ex)
            {
                OnException(filterContext, ex);
            }
        }

        /// <summary>
        /// When overridden it provides access to the known user object.  
        /// E.g. to persist Queue ID details to the database.
        /// </summary>
        /// <param name="filterContext">The Action Executing Filter Context</param>
        /// <param name="knownUser">The validated Known User object</param>
        /// <example>
        /// <code>
        /// protected override void OnValidated(ActionExecutingContext filterContext, IKnownUser knownUser)
        /// {
        ///         PersistModel model = new PersistModel(
        ///             knownUser.QueueId,
        ///             knownUser.PlaceInQueue,
        ///             knownUser.TimeStamp);
        /// 
        ///         model.Persist();
        ///     }
        /// 
        ///     base.OnValidated(filterContext, result);
        /// }
        /// </code>
        /// </example>
        protected virtual void OnValidated(ActionExecutingContext filterContext, IKnownUser knownUser)
        {
        }

        /// <summary>
        /// When overridden it provides access to Known User Validation exceptions.  
        /// </summary>
        /// <param name="filterContext">The Action Executing Filter Context</param>
        /// <param name="exception">The exception thrown</param>
        protected virtual void OnException(ActionExecutingContext filterContext, KnownUserException exception)
        {
            KnownUserErrorModel model = new KnownUserErrorModel(exception);
            filterContext.Result = new ViewResult()
            {
                ViewName = this.ErrorView,
                ViewData = new ViewDataDictionary(model)
            };
        }
    }
}
