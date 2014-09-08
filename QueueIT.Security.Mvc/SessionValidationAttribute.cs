using System;
using System.Web.Mvc;

namespace QueueIT.Security.Mvc
{
    /// <summary>
    /// Action Filter which enables simple implementation of the QueueIT.Security functionality.
    /// Please be aware that this filter is not applied to error controller actions or similar which will cause users to get looped arround.
    /// </summary>
    /// <remarks>
    /// View members for additional information and examples
    /// </remarks>
    /// <example>
    /// Source Code;
    /// <code>
    /// [SessionValidation("advanced")]
    /// public ActionResult Index(SessionValidationModel validationModel)
    /// {
    ///     IValidateResult result = validationModel.ValidateResult;
    /// 
    ///     return View();
    /// }
    /// </code>
    /// 
    /// Configuration:
    /// <code>
    /// <![CDATA[
    /// <configuration>
    ///    <configSections>
    ///       <section name="queueit.security" type="QueueIT.Security.Configuration.SettingsSection, QueueIT.Security"/>
    ///    </configSections>
    ///    <queueit.security 
    ///       secretKey="a774b1e2-8da7-4d51-b1a9-7647147bb13bace77210-a488-4b6f-afc9-8ba94551a7d7">
    ///       <queues>
    ///          <queue name="default" customerId="ticketania" eventId="simple"/>
    ///       </queues>
    ///    </queueit.security>
    /// </configuration>
    /// ]]>
    /// </code>
    /// </example>    
    public class SessionValidationAttribute : ActionFilterAttribute 
    {
        private readonly string _queueName;
        private readonly string _customerId;
        private readonly string _eventId;
        private bool? _sslEnabled;
        private bool? _includeTargetUrl;
        private string _domainAlias;

        /// <summary>
        /// If true the user will be redirected to the current page when the user is through the queue
        /// </summary>
        public bool IncludeTargetUrl
        {
            get { throw new NotImplementedException();}
            set { this._includeTargetUrl = value; }
        }
        /// <summary>
        /// If true the queue uses SSL
        /// </summary>
        public bool SslEnabled
        {
            get { throw new NotImplementedException(); }
            set { this._sslEnabled = value; }
        }
        /// <summary>
        /// An optional domain of the queue
        /// </summary>
        public string DomainAlias
        {
            get { throw new NotImplementedException(); }
            set { this._domainAlias = value; }
        }
        /// <summary>
        /// The view to display in case of a Known User validation exception. Default view is QueueITValidationError.
        /// </summary>
        public string ErrorView { get; set; }

        /// <summary>
        /// Validates the request based on the default queue defined by configuration 
        /// This method requires a queue with then name 'default' to be configured in the application config file
        /// </summary>
        /// <example>
        /// Source Code;
        /// <code>
        /// [SessionValidation]
        /// public ActionResult Index()
        /// {
        ///     return View();
        /// }
        /// </code>
        /// 
        /// Configuration:
        /// <code>
        /// <![CDATA[
        /// <configuration>
        ///    <configSections>
        ///       <section name="queueit.security" type="QueueIT.Security.Configuration.SettingsSection, QueueIT.Security"/>
        ///    </configSections>
        ///    <queueit.security 
        ///       secretKey="a774b1e2-8da7-4d51-b1a9-7647147bb13bace77210-a488-4b6f-afc9-8ba94551a7d7">
        ///       <queues>
        ///          <queue name="default" customerId="ticketania" eventId="simple"/>
        ///       </queues>
        ///    </queueit.security>
        /// </configuration>
        /// ]]>
        /// </code>
        /// </example>
        public SessionValidationAttribute()
        {
            this.ErrorView = "QueueITValidationError";
        }


        /// <summary>
        /// Validates the request based on a queue defined by configuration  
        /// This method requires a queue to be configured in the application config file with the name provided in queueName
        /// </summary>
        /// <param name="queueName">The name of the queue as defined in the configuration file</param>
        /// <example>
        /// Source Code;
        /// <code>
        /// [SessionValidation("advanced")]
        /// public ActionResult Index()
        /// {
        ///     return View();
        /// }
        /// </code>
        /// 
        /// Configuration:
        /// <code>
        /// <![CDATA[
        /// <configuration>
        ///    <configSections>
        ///       <section name="queueit.security" type="QueueIT.Security.Configuration.SettingsSection, QueueIT.Security"/>
        ///    </configSections>
        ///    <queueit.security 
        ///       secretKey="a774b1e2-8da7-4d51-b1a9-7647147bb13bace77210-a488-4b6f-afc9-8ba94551a7d7">
        ///       <queues>
        ///          <queue name="advanced" customerId="ticketania" eventId="advanced"/>
        ///       </queues>
        ///    </queueit.security>
        /// </configuration>
        /// ]]>
        /// </code>
        /// </example>
        public SessionValidationAttribute(string queueName)
            : this()
        {
            _queueName = queueName;
        }

        /// <summary>
        /// Validates the request not using configuration 
        /// </summary>
        /// <param name="customerId">The Customer ID of the queue</param>
        /// <param name="eventId">The Event ID of the queue</param>
        /// <example>
        /// Source Code;
        /// <code>
        /// [SessionValidation("ticketania", "codeonly")]
        /// public ActionResult Index()
        /// {
        ///     return View();
        /// }
        /// </code>
        /// </example>
        public SessionValidationAttribute(string customerId, string eventId)
            : this()
        {
            _customerId = customerId;
            _eventId = eventId;
        }

        /// <summary>
        /// Validates the request
        /// </summary>
        /// <param name="filterContext">The Action Executing Filter Context</param>
        public sealed override void OnActionExecuting(ActionExecutingContext filterContext)
        {
            try
            {
                IValidateResult result = ValidateRequest(filterContext);

                OnValidated(filterContext, result);

                foreach (var value in filterContext.ActionParameters.Values)
                {
                    if (value is SessionValidationModel)
                        (value as SessionValidationModel).ValidateResult = result;
                }
            }
            catch (ExpiredValidationException ex)
            {
                OnException(filterContext, ex);

                SessionValidationErrorModel model = new SessionValidationErrorModel(ex.Queue, ex, ex.KnownUser.OriginalUrl);
                filterContext.Result = new ViewResult()
                {
                    ViewName = this.ErrorView,
                    ViewData = new ViewDataDictionary(model)
                };
            }
            catch (KnownUserValidationException ex)
            {
                OnException(filterContext, ex);

                SessionValidationErrorModel model = new SessionValidationErrorModel(
                    ex.Queue, 
                    ex, 
                    (ex.InnerException as KnownUserException).OriginalUrl);

                filterContext.Result = new ViewResult()
                {
                    ViewName = this.ErrorView,
                    ViewData = new ViewDataDictionary(model)
                };
            }
        }

        /// <summary>
        /// When overridden it provides full control of generating the validation result.  
        /// E.g. by looking up the Customer and Event ID in a database
        /// </summary>
        /// <param name="filterContext">The Action Executing Filter Context</param>
        /// <returns>The validation result</returns>
        /// <example>
        /// <code>
        /// protected override IValidateResult ValidateRequest(ActionExecutingContext filterContext)
        /// {
        ///    var model = db.QueueLookup(filterContext);
        ///    return SessionValidationController.ValidateRequest(model.CustomerId, model.EventId);
        /// }
        /// </code>
        /// </example>
        protected virtual IValidateResult ValidateRequest(ActionExecutingContext filterContext)
        {
            if (!string.IsNullOrEmpty(this._queueName))
                return SessionValidationController.ValidateRequest(
                    this._queueName, this._includeTargetUrl, this._sslEnabled, this._domainAlias);
            if (!string.IsNullOrEmpty(this._customerId) && !string.IsNullOrEmpty(this._eventId))
                return SessionValidationController.ValidateRequest(
                    this._customerId, this._eventId, this._includeTargetUrl, this._sslEnabled, this._domainAlias);
            
            return SessionValidationController.ValidateRequest(this._includeTargetUrl, this._sslEnabled, this._domainAlias);
        }

        /// <summary>
        /// When overridden it provides access to the validation request.  
        /// E.g. to persist Queue ID details to the database.
        /// Call base to redirect to the queue on EnqueueResult.
        /// </summary>
        /// <param name="filterContext">The Action Executing Filter Context</param>
        /// <param name="result">The validation result</param>
        /// <example>
        /// <code>
        /// protected override void OnValidated(ActionExecutingContext filterContext, IValidateResult result)
        /// {
        ///     // Check if user must be enqueued
        ///     if (result is EnqueueResult)
        ///     {
        ///         // Optional action
        ///     }
        /// 
        ///     // Check if user has been through the queue (will be invoked for every page request after the user has been validated)
        ///     if (result is AcceptedConfirmedResult)
        ///     {
        ///         AcceptedConfirmedResult confirmedResult = result as AcceptedConfirmedResult;
        /// 
        ///         if (!confirmedResult.IsInitialValidationRequest)
        ///             return; // data has already been persisted
        /// 
        ///         PersistModel model = new PersistModel(
        ///             confirmedResult.Queue.CustomerId,
        ///             confirmedResult.Queue.EventId,
        ///             confirmedResult.KnownUser.QueueId,
        ///             confirmedResult.KnownUser.PlaceInQueue,
        ///             confirmedResult.KnownUser.TimeStamp);
        /// 
        ///         model.Persist();
        ///     }
        /// 
        ///     base.OnValidated(filterContext, result);
        /// }
        /// </code>
        /// </example>
        protected virtual void OnValidated(ActionExecutingContext filterContext, IValidateResult result)
        {
            EnqueueResult enqueueResult = result as EnqueueResult;
            if (enqueueResult != null)
            {
                filterContext.Result = new RedirectResult(enqueueResult.RedirectUrl.AbsoluteUri);
            }
        }

        /// <summary>
        /// When overridden it provides access to Known User Validation exceptions.  
        /// </summary>
        /// <param name="filterContext">The Action Executing Filter Context</param>
        /// <param name="exception">The exception thrown</param>
        protected virtual void OnException(ActionExecutingContext filterContext, SessionValidationException exception)
        {
        }
    }
}
