using System;
using System.Globalization;
using QueueIT.Security.Configuration;

namespace QueueIT.Security
{
    /// <summary>
    /// Controller class which enables simple implementation of the QueueIT.Security functionality
    /// </summary>
    /// <remarks>
    /// The SessionValidationController will by default add a session cookie when users have been through the queue to store data. 
    /// The cookie will only be in scope of the domain of the request (e.g. www.ticketania.com). 
    /// Please add the 'CookieDomain' setting, as shown in the examples (see ValidateRequest() members), if your website uses multiple subdomains.
    /// <br/><br/>
    /// There is also an option for using the standard ASP.NET Session state to store data using the SessionValidationController.Configure() method. 
    /// Please note that session state must be replicated between servers in a webfarm. See http://msdn.microsoft.com/library/ms178586.aspx
    /// <code>
    /// <![CDATA[
    /// //.Net Framework
    /// SessionValidationController.Configure(validationResultProviderFactory: () => new SessionValidateResultRepository());
    /// ]]>
    /// </code>
    /// <code>
    /// <![CDATA[
    /// //Java EE
    /// SessionValidationController.Configure(null, new Callable<IValidateResultRepository>() {
    ///     public IValidateResultRepository call() {
    ///         return new SessionValidateResultRepository();
    ///     }
    /// });
    /// ]]>
    /// </code>
    /// A thrid option is to implement a new validation result provider by implementing the IValidateResultRepository repository.
    /// <br/><br/>
    /// View members for additional information and examples
    /// </remarks>
    public static class SessionValidationController
    {
        private static TimeSpan _defaultTicketExpiration;
        private static readonly IValidateResultRepository _defaultValidationResultRepository = new CookieValidateResultRepository();
        private static Func<IValidateResultRepository> _validationResultProviderFactory = () => _defaultValidationResultRepository;

        static SessionValidationController()
        {
            SettingsSection settings = SettingsSection.GetSection();
            if (settings != null)
            {
                _defaultTicketExpiration = settings.TicketExpiration;
            }
        }

        /// <summary>
        /// Configures the SessionValidationController. This method will override any previous calls and coniguration in config files.
        /// </summary>
        /// <param name="ticketExpiration">The time Known User request urls are valid after they have been issued. Default is 3 minutes.</param>
        /// <param name="validationResultProviderFactory">
        /// Factory for creating a repository for storing user validation state. 
        /// The default implementation uses the built-in sessions.
        /// </param>
        public static void Configure(
            TimeSpan ticketExpiration = default(TimeSpan), 
            Func<IValidateResultRepository> validationResultProviderFactory = null)
        {
            if (ticketExpiration != default(TimeSpan))
                _defaultTicketExpiration = ticketExpiration;
            if (validationResultProviderFactory != null)
                _validationResultProviderFactory = validationResultProviderFactory;
        }

        /// <summary>
        /// Validates the request based on the default queue defined by configuration 
        /// This method requires a queue with then name 'default' to be configured in the application config file
        /// </summary>
        /// <param name="includeTargetUrl">
        /// If true the user will be redirected to the current page when the user is through the queue
        /// </param>
        /// <param name="sslEnabled">
        /// If true the queue uses SSL
        /// </param>
        /// <param name="domainAlias">
        /// An optional domain of the queue
        /// </param>
        /// <param name="language">
        /// The language of the queue if different from default
        /// </param>
        /// <param name="layoutName">
        /// The layout of the queue if different from default
        /// </param>
        /// <exception cref="ExpiredValidationException">The Known User request URL has expired</exception>
        /// <exception cref="KnownUserValidationException">The Known User request URL is invalid or has been tampered with</exception>
        /// <returns>The validation result</returns>
        /// <example>
        /// <code language="cs">
        /// try
        /// {
        ///     IValidateResult result = SessionValidationController.ValidateRequest();
        ///
        ///     // Check if user must be enqueued
        ///     if (result is EnqueueResult)
        ///     {
        ///         Response.Redirect((result as EnqueueResult).RedirectUrl.AbsoluteUri);
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
        /// }
        /// catch (ExpiredValidationException ex)
        /// {
        ///     // Known user has has expired - Show error page and use GetCancelUrl to get user back in the queue
        ///     Response.Redirect("Error.aspx?queuename=advanced&amp;t=" + HttpUtility.UrlEncode(ex.KnownUser.OriginalUrl.AbsoluteUri));
        /// }
        /// catch (KnownUserValidationException ex)
        /// {
        ///     // The known user url or hash is not valid - Show error page and use GetCancelUrl to get user back in the queue
        ///     Response.Redirect("Error.aspx?queuename=advanced&amp;t=" + HttpUtility.UrlEncode(ex.OriginalUrl.AbsoluteUri));
        /// }        
        /// </code>
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
        ///       <repositorySettings>
        ///           <setting name="CookieDomain" value=".ticketania.com" />
        ///       </repositorySettings>
        ///    </queueit.security>
        /// </configuration>
        /// ]]>
        /// </code>
        /// </example>
        /// <example>
        /// PHP Example
        /// <code language="none">
        /// <![CDATA[
        /// <?php
        /// 	require_once('../QueueIT.Security PHP/SessionValidationController.php');
        /// 		
        /// 	use QueueIT\Security\SessionValidationController, 
        /// 		QueueIT\Security\ExpiredValidationException, 
        /// 		QueueIT\Security\KnownUserValidationException,
        /// 		QueueIT\Security\EnqueueResult;
        /// 
        /// 	session_start();
        /// 
        /// 	try
        /// 	{
        /// 		$result = SessionValidationController::validateRequestFromConfiguration();
        /// 		
        /// 		// Check if user must be enqueued
        /// 		if ($result instanceof EnqueueResult)
        /// 		{
        /// 			header('Location: ' . $result->getRedirectUrl());
        /// 		}
        /// 	}
        /// 	catch (ExpiredValidationException $ex)
        /// 	{
        /// 		// Known user has has expired - Show error page and use GetCancelUrl to get user back in the queue
        /// 		header('Location: error.php?queuename=default&t=' . urlencode($ex->getKnownUser()->getOriginalUrl()));
        /// 	}
        /// 	catch (KnownUserValidationException $ex)
        /// 	{
        /// 		// Known user is invalid - Show error page and use GetCancelUrl to get user back in the queue
        /// 		header('Location: error.php?queuename=default&t=' + urlencode($ex->previous->getOriginalUrl()));
        /// 	}
        /// ?>
        /// ]]>
        /// </code>
        /// Configuration:
        /// <code>
        /// <![CDATA[
        /// [settings]
        /// secretKey = a774b1e2-8da7-4d51-b1a9-7647147bb13bace77210-a488-4b6f-afc9-8ba94551a7d7
        /// 
        /// [default]
        /// customerId = ticketania
        /// eventId = simple
        /// ]]>
        /// </code>
        /// </example>
        /// <example>
        /// Java EE Example
        /// <code language="none">
        /// <![CDATA[
        ///     try
        ///     {
        ///         IValidateResult result = SessionValidationController.validateRequest();
        /// 
        ///         // Check if user must be enqueued
        ///         if (result instanceof EnqueueResult)
        ///         {
        ///             response.sendRedirect(((EnqueueResult)result).getRedirectUrl().toString());
        ///             return;
        ///         }
        ///     }
        ///     catch (ExpiredValidationException ex)
        ///     {
        ///         // Known user has has expired - Show error page and use GetCancelUrl to get user back in the queue
        ///         response.sendRedirect("error.jsp?queuename=default&t=" + ex.getKnownUser().getOriginalUrl());
        ///         return;
        ///     }
        ///     catch (KnownUserValidationException ex)
        ///     {
        ///         // Known user is invalid - Show error page and use GetCancelUrl to get user back in the queue
        ///         response.sendRedirect("error.jsp?queuename=default&t=" + ((KnownUserException)ex.getCause()).getOriginalUrl());
        ///         return;
        ///     }
        /// ]]>
        /// </code>
        /// Configuration queueit.properties:
        /// <code>
        /// <![CDATA[
        /// secretKey = a774b1e2-8da7-4d51-b1a9-7647147bb13bace77210-a488-4b6f-afc9-8ba94551a7d7
        /// ]]>
        /// </code>
        /// Configuration queueit-default.properties:
        /// <code>
        /// <![CDATA[
        /// customerId = ticketania
        /// eventId = simple
        /// ]]>
        /// </code>
        /// </example>
        public static IValidateResult ValidateRequest(
            bool? includeTargetUrl = null,
            bool? sslEnabled = null,
            string domainAlias = null,
            CultureInfo language = null, 
            string layoutName = null)
        {
            Queue queue = QueueFactory.CreateQueue() as Queue;

            return ValidateRequest(
                queue,
                sslEnabled.HasValue ? sslEnabled.Value : queue.DefaultSslEnabled,
                includeTargetUrl.HasValue ? includeTargetUrl.Value : queue.DefaultIncludeTargetUrl,
                domainAlias ?? queue.DefaultDomainAlias,
                language ?? queue.DefaultLanguage,
                layoutName ?? queue.DefaultLayoutName);
        }

        /// <summary>
        /// Validates the request based on a queue defined by configuration  
        /// This method requires a queue to be configured in the application config file with the name provided in queueName
        /// </summary>
        /// <param name="queueName">The name of the queue as defined in the configuration file</param>
        /// <param name="includeTargetUrl">
        /// If true the user will be redirected to the current page when the user is through the queue
        /// </param>
        /// <param name="sslEnabled">
        /// If true the queue uses SSL
        /// </param>
        /// <param name="domainAlias">
        /// An optional domain of the queue
        /// </param>
        /// <param name="language">
        /// The language of the queue if different from default
        /// </param>
        /// <param name="layoutName">
        /// The layout of the queue if different from default
        /// </param>
        /// <exception cref="ExpiredValidationException">The Known User request URL has expired</exception>
        /// <exception cref="KnownUserValidationException">The Known User request URL is invalid or has been tampered with</exception>
        /// <returns>The validation result</returns>
        /// <example>
        /// Source Code;
        /// <code language="cs">
        /// try
        /// {
        ///     IValidateResult result = SessionValidationController.ValidateRequest("advanced");
        ///
        ///     // Check if user must be enqueued
        ///     if (result is EnqueueResult)
        ///     {
        ///         Response.Redirect((result as EnqueueResult).RedirectUrl.AbsoluteUri);
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
        /// }
        /// catch (ExpiredValidationException ex)
        /// {
        ///     // Known user has has expired - Show error page and use GetCancelUrl to get user back in the queue
        ///     Response.Redirect("Error.aspx?queuename=advanced&amp;t=" + HttpUtility.UrlEncode(ex.KnownUser.OriginalUrl.AbsoluteUri));
        /// }
        /// catch (KnownUserValidationException ex)
        /// {
        ///     // The known user url or hash is not valid - Show error page and use GetCancelUrl to get user back in the queue
        ///     Response.Redirect("Error.aspx?queuename=advanced&amp;t=" + HttpUtility.UrlEncode(ex.OriginalUrl.AbsoluteUri));
        /// }        
        /// </code>
        /// 
        /// Configuration:
        /// <code language="config">
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
        ///       <repositorySettings>
        ///           <setting name="CookieDomain" value=".ticketania.com" />
        ///       </repositorySettings>
        ///    </queueit.security>
        /// </configuration>
        /// ]]>
        /// </code>
        /// </example>
        /// <example>
        /// PHP Example
        /// <code language="none">
        /// <![CDATA[
        /// <?php
        /// 	require_once('../QueueIT.Security PHP/SessionValidationController.php');
        /// 		
        /// 	use QueueIT\Security\SessionValidationController, 
        /// 		QueueIT\Security\ExpiredValidationException, 
        /// 		QueueIT\Security\KnownUserValidationException,
        /// 		QueueIT\Security\EnqueueResult;
        /// 
        /// 	session_start();
        /// 
        /// 	try
        /// 	{
        /// 		$result = SessionValidationController::validateRequestFromConfiguration('advanced');
        /// 		
        /// 		// Check if user must be enqueued
        /// 		if ($result instanceof EnqueueResult)
        /// 		{
        /// 			header('Location: ' . $result->getRedirectUrl());
        /// 		}
        /// 		
        /// 		// Check if user has been through the queue (will be invoked for every page request after the user has been validated)
        /// 		if ($result instanceof AcceptedConfirmedResult)
        /// 		{		
        /// 			if ($result->isInitialValidationRequest())
        /// 			{
        /// 				$model = array(
        /// 					'CustomerId' => $result->getQueue()->getCustomerId(),
        /// 					'EventId' => $result->getQueue()->getEventId(),
        /// 					'QueueId' => $result->getKnownUser()->getQueueId(),
        /// 					'PlaceInQueue' => $result->getKnownUser()->getPlaceInQueue(),
        /// 					'TimeStamp' => $result->getKnownUser()->getTimeStamp());
        /// 			}
        /// 		}  
        /// 	}
        /// 	catch (ExpiredValidationException $ex)
        /// 	{
        /// 		// Known user has has expired - Show error page and use GetCancelUrl to get user back in the queue
        /// 		header('Location: error.php?queuename=default&t=' . urlencode($ex->getKnownUser()->getOriginalUrl()));
        /// 	}
        /// 	catch (KnownUserValidationException $ex)
        /// 	{
        /// 		// Known user is invalid - Show error page and use GetCancelUrl to get user back in the queue
        /// 		header('Location: error.php?queuename=default&t=' + urlencode($ex->previous->getOriginalUrl()));
        /// 	}        
        /// ?>
        /// ]]>
        /// </code>
        /// Configuration:
        /// <code>
        /// <![CDATA[
        /// [settings]
        /// secretKey = a774b1e2-8da7-4d51-b1a9-7647147bb13bace77210-a488-4b6f-afc9-8ba94551a7d7
        /// 
        /// [advanced]
        /// customerId = ticketania
        /// eventId = advanced
        /// includeTargetUrl = true
        /// domainAlias = queue-example.ticketania.com
        /// landingPage = http://www.mysplitpage.com/
        /// useSsl = false
        /// ]]>
        /// </code>
        /// </example>
        /// <example>
        /// Java EE Example
        /// <code language="none">
        /// <![CDATA[
        ///     try
        ///     {
        ///             IValidateResult result = SessionValidationController.validateRequest("advanced");
        /// 
        ///             // Check if user must be enqueued
        ///             if (result instanceof EnqueueResult)
        ///             {
        ///                 response.sendRedirect(((EnqueueResult)result).getRedirectUrl().toString());
        ///                 return;
        ///             }
        /// 
        ///             // Check if user has been through the queue (will be invoked for every page request after the user has been validated)
        ///             if (result instanceof AcceptedConfirmedResult)
        ///             {
        ///                 AcceptedConfirmedResult accepted = (AcceptedConfirmedResult)result;
        ///                     if (accepted.isInitialValidationRequest())
        ///                     {
        ///                         Object[] model = new Object[] {
        ///                                     accepted.getQueue().getCustomerId(),
        ///                                     accepted.getQueue().getEventId(),
        ///                                     accepted.getKnownUser().getQueueId(),
        ///                                     accepted.getKnownUser().getPlaceInQueue(),
        ///                                     accepted.getKnownUser().getTimeStamp()
        ///                         };
        ///                     }
        ///             }
        ///     }
        ///     catch (ExpiredValidationException ex)
        ///     {
        ///         // Known user has has expired - Show error page and use GetCancelUrl to get user back in the queue
        ///         response.sendRedirect("error.jsp?queuename=advanced&t=" + ex.getKnownUser().getOriginalUrl());
        ///         return;
        ///     }
        ///     catch (KnownUserValidationException ex)
        ///     {
        ///         // Known user is invalid - Show error page and use GetCancelUrl to get user back in the queue
        ///         response.sendRedirect("error.jsp?queuename=advanced&t=" + ((KnownUserException)ex.getCause()).getOriginalUrl());
        ///         return;
        ///     }
        /// ]]>
        /// </code>
        /// Configuration queueit.properties:
        /// <code>
        /// <![CDATA[
        /// secretKey = a774b1e2-8da7-4d51-b1a9-7647147bb13bace77210-a488-4b6f-afc9-8ba94551a7d7
        /// ]]>
        /// </code>
        /// Configuration queueit-default.properties:
        /// <code>
        /// <![CDATA[
        /// customerId = ticketania
        /// eventId = advanced
        /// includeTargetUrl = true
        /// domainAlias = queue-example.ticketania.com
        /// landingPage = QueueIT.Security.Examples.Java/advancedlanding.jsp
        /// useSsl = false
        /// ]]>
        /// </code>
        /// </example>        
        public static IValidateResult ValidateRequest(
            string queueName,
            bool? includeTargetUrl = null,
            bool? sslEnabled = null,
            string domainAlias = null,
            CultureInfo language = null, 
            string layoutName = null)
        {
            if (string.IsNullOrEmpty(queueName))
                throw new ArgumentException("Queue name is required", "queueName");

            Queue queue = QueueFactory.CreateQueue(queueName) as Queue;

            return ValidateRequest(
                queue,
                sslEnabled.HasValue ? sslEnabled.Value : queue.DefaultSslEnabled,
                includeTargetUrl.HasValue ? includeTargetUrl.Value : queue.DefaultIncludeTargetUrl,
                domainAlias ?? queue.DefaultDomainAlias,
                language ?? queue.DefaultLanguage,
                layoutName ?? queue.DefaultLayoutName);
        }

        /// <summary>
        /// Validates the request not using configuration 
        /// </summary>
        /// <param name="customerId">The Customer ID of the queue</param>
        /// <param name="eventId">The Event ID of the queue</param>
        /// <param name="includeTargetUrl">
        /// If true the user will be redirected to the current page when the user is through the queue
        /// </param>
        /// <param name="sslEnabled">
        /// If true the queue uses SSL
        /// </param>
        /// <param name="domainAlias">
        /// An optional domain of the queue
        /// </param>
        /// <param name="language">
        /// The language of the queue if different from default
        /// </param>
        /// <param name="layoutName">
        /// The layout of the queue if different from default
        /// </param>
        /// <exception cref="ExpiredValidationException">The Known User request URL has expired</exception>
        /// <exception cref="KnownUserValidationException">The Known User request URL is invalid or has been tampered with</exception>
        /// <returns>The validation result</returns>
        /// <example>
        /// Source Code;
        /// <code language="cs">
        /// // Setting cookie domain to allow multiple subdomains in your application 
        /// // May be placed in global.asax
        /// CookieValidateResultRepository.Configure(cookieDomain: ".ticketania.com");
        /// 
        /// try
        /// {
        ///     IValidateResult result = SessionValidationController.ValidateRequest("ticketania", "codeonly");
        ///
        ///     // Check if user must be enqueued
        ///     if (result is EnqueueResult)
        ///     {
        ///         Response.Redirect((result as EnqueueResult).RedirectUrl.AbsoluteUri);
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
        /// }
        /// catch (ExpiredValidationException ex)
        /// {
        ///     // Known user has has expired - Show error page and use GetCancelUrl to get user back in the queue
        ///     Response.Redirect("Error.aspx?queuename=advanced&amp;t=" + HttpUtility.UrlEncode(ex.KnownUser.OriginalUrl.AbsoluteUri));
        /// }
        /// catch (KnownUserValidationException ex)
        /// {
        ///     // The known user url or hash is not valid - Show error page and use GetCancelUrl to get user back in the queue
        ///     Response.Redirect("Error.aspx?queuename=advanced&amp;t=" + HttpUtility.UrlEncode(ex.OriginalUrl.AbsoluteUri));
        /// }        
        /// </code>
        /// </example>
        /// <example>
        /// PHP Example
        /// <code language="none">
        /// <![CDATA[
        /// <?php
        /// 	require_once('../QueueIT.Security PHP/SessionValidationController.php');
        /// 		
        /// 	use QueueIT\Security\SessionValidationController, 
        /// 		QueueIT\Security\ExpiredValidationException, 
        /// 		QueueIT\Security\KnownUserValidationException,
        /// 		QueueIT\Security\EnqueueResult;
        /// 
        /// 	session_start();
        /// 
        /// 	KnownUserFactory::configure('a774b1e2-8da7-4d51-b1a9-7647147bb13bace77210-a488-4b6f-afc9-8ba94551a7d7');
        /// 	
        /// 	try
        /// 	{
        /// 		$result = SessionValidationController::validateRequest('ticketania', 'codeonly', true);
        /// 		
        /// 		// Check if user must be enqueued
        /// 		if ($result instanceof EnqueueResult)
        /// 		{
        /// 			header('Location: ' . $result->getRedirectUrl());
        /// 		}
        /// 	}
        /// 	catch (ExpiredValidationException $ex)
        /// 	{
        /// 		// Known user has has expired - Show error page and use GetCancelUrl to get user back in the queue
        /// 		header('Location: error.php?queuename=default&t=' . urlencode($ex->getKnownUser()->getOriginalUrl()));
        /// 	}
        /// 	catch (KnownUserValidationException $ex)
        /// 	{
        /// 		// Known user is invalid - Show error page and use GetCancelUrl to get user back in the queue
        /// 		header('Location: error.php?queuename=default&t=' + urlencode($ex->previous->getOriginalUrl()));
        /// 	}     
        /// ?>
        /// ]]>
        /// </code>
        /// </example>
        /// <example>
        /// Java EE Example
        /// <code language="none">
        /// <![CDATA[
        ///     KnownUserFactory.configure("a774b1e2-8da7-4d51-b1a9-7647147bb13bace77210-a488-4b6f-afc9-8ba94551a7d7");
        /// 
        ///     try
        ///     {
        ///         IValidateResult result = SessionValidationController.validateRequest("ticketania", "codeonly", true);
        /// 
        ///         // Check if user must be enqueued
        ///         if (result instanceof EnqueueResult)
        ///         {
        ///             response.sendRedirect(((EnqueueResult)result).getRedirectUrl().toString());
        ///             return;
        ///         }
        ///     }
        ///     catch (ExpiredValidationException ex)
        ///     {
        ///         // Known user has has expired - Show error page and use GetCancelUrl to get user back in the queue
        ///          response.sendRedirect("error.jsp?queuename=&t=" + ex.getKnownUser().getOriginalUrl());
        ///         return;
        ///     }
        ///     catch (KnownUserValidationException ex)
        ///     {
        ///         // Known user is invalid - Show error page and use GetCancelUrl to get user back in the queue
        ///         response.sendRedirect("error.jsp?queuename=&t=" + ((KnownUserException)ex.getCause()).getOriginalUrl());
        ///         return;
        ///     }        
        /// ]]>
        /// </code>
        /// </example>
        public static IValidateResult ValidateRequest(
            string customerId, 
            string eventId,
            bool? includeTargetUrl = null,
            bool? sslEnabled = null,
            string domainAlias = null,
            CultureInfo language = null, 
            string layoutName = null)
        {
            if (string.IsNullOrEmpty(customerId))
                throw new ArgumentException("Customer ID is required", "customerId");
            if (string.IsNullOrEmpty(eventId))
                throw new ArgumentException("Event ID is required", "eventId");

            Queue queue = QueueFactory.CreateQueue(customerId.ToLower(), eventId.ToLower()) as Queue;

            return ValidateRequest(
                queue, 
                sslEnabled.HasValue ? sslEnabled.Value : queue.DefaultSslEnabled, 
                includeTargetUrl.HasValue ? includeTargetUrl.Value : queue.DefaultIncludeTargetUrl,
                domainAlias ?? queue.DefaultDomainAlias,
                language ?? queue.DefaultLanguage,
                layoutName ?? queue.DefaultLayoutName);
        }

        private static IValidateResult ValidateRequest(
            Queue queue, 
            bool sslEnabled, 
            bool includeTargetUrl, 
            string domainAlias,
            CultureInfo language = null, 
            string layoutName = null)
        {
            IValidateResult sessionObject = _validationResultProviderFactory.Invoke().GetValidationResult(queue);
            if (sessionObject != null)
            {
                AcceptedConfirmedResult confirmedResult = sessionObject as AcceptedConfirmedResult;
                if (confirmedResult != null)
                    return new AcceptedConfirmedResult(queue, confirmedResult.KnownUser, false);

                return sessionObject;
            }
            try
            {
                IKnownUser knownUser = KnownUserFactory.VerifyMd5Hash();

                if (knownUser == null)
                {
                    Uri landingPage = queue.GetLandingPageUrl(includeTargetUrl);

                    if (landingPage != null)
                        return new EnqueueResult(queue, landingPage);

                    return new EnqueueResult(queue, queue.GetQueueUrl(includeTargetUrl, sslEnabled, domainAlias, language, layoutName));
                }
                if (_defaultTicketExpiration != default(TimeSpan) && knownUser.TimeStamp <= DateTime.UtcNow.Subtract(_defaultTicketExpiration))
                {
                    throw new ExpiredValidationException(queue, knownUser);
                }

                AcceptedConfirmedResult result = new AcceptedConfirmedResult(queue, knownUser, true);
                _validationResultProviderFactory.Invoke().SetValidationResult(queue, result);

                return result;
            }
            catch (InvalidKnownUserUrlException ex)
            {
                throw new KnownUserValidationException(ex, queue);
            }
            catch (InvalidKnownUserHashException ex)
            {
                throw new KnownUserValidationException(ex, queue);
            }

        }
    }
}
