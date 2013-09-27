using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;

namespace QueueIT.Security
{
    /// <summary>
    /// An object reprecenting the queue, the status of the queue and helper methods to generate URLs
    /// </summary>
    public interface IQueue
    {
        /// <summary>
        /// The Event ID of the queue
        /// </summary>
        string EventId { get; }

        /// <summary>
        /// The Customer ID of the queue
        /// </summary>
        string CustomerId { get; }

        /// <summary>
        /// Returns the URL to the queue. This will be the landing page if one is configured
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
        /// <returns>The URL to the queue</returns>
        Uri GetQueueUrl(bool? includeTargetUrl = null, bool? sslEnabled = null, string domainAlias = null, CultureInfo language = null, string layoutName = null);

        /// <summary>
        /// Returns the URL to the queue. This will be the landing page if one is configured
        /// </summary>
        /// <param name="targetUrl">
        /// If URL the user will be redirected to when the user is through the queue
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
        /// <returns>The URL to the queue</returns>
        Uri GetQueueUrl(Uri targetUrl, bool? sslEnabled = null, string domainAlias = null, CultureInfo language = null, string layoutName = null);

        /// <summary>
        /// Returns the URL used to cancel a Queue ID and force the user back in the queue
        /// </summary>
        /// <param name="landingPage">
        /// The URL the user is redirected to when the Queue ID is canceled. 
        /// If null the user is redirected to the queue
        /// </param>
        /// <param name="queueId">The Queue ID to cancel</param>
        /// <param name="sslEnabled">If true the cancel page uses SSL</param>
        /// <param name="domainAlias">An optional domain of the cancel page</param>
        /// <returns>The URL of the cancel page</returns>
        Uri GetCancelUrl(Uri landingPage = null, Guid? queueId = null, bool? sslEnabled = null, string domainAlias = null);

        /// <summary>
        /// Returns the configured landing page (split page)
        /// </summary>
        /// <param name="includeTargetUrl">If true the current URL will be sent to the landing page as a querystring parameter</param>
        /// <returns>The URL of the landing page</returns>
        Uri GetLandingPageUrl(bool? includeTargetUrl = null);

        /// <summary>
        /// Returns the configured landing page (split page)
        /// </summary>
        /// <param name="targetUrl">An URL to send to the landing page as a querystring parameter</param>
        /// <returns>The URL of the landing page</returns>
        Uri GetLandingPageUrl(Uri targetUrl);
    }
}
