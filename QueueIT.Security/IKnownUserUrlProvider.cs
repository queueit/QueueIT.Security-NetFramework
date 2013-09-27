using System;
using System.Web;

namespace QueueIT.Security
{
    /// <summary>
    /// Interface to create a class that returns the original hashed URL (in case of URL rewrite or similar)
    /// </summary>
    public interface IKnownUserUrlProvider
    {
        /// <summary>
        /// Returns the redirect URL as provided by Queue-it
        /// </summary>
        /// <returns>The url sent to the browser by the Queue-it service</returns>
        Uri GetUrl();
        /// <summary>
        /// Returns the Queue ID from the Known User token in the URL querystring
        /// </summary>
        /// <param name="queryStringPrefix">The querystring prefix</param>
        /// <returns>The Queue ID</returns>
        string GetQueueId(string queryStringPrefix);
	    /// <summary>
        /// Returns the obfuscated place in queue from the Known User token in the URL querystring
	    /// </summary>
        /// <param name="queryStringPrefix">The querystring prefix</param>
        /// <returns>The obfuscated place in queue</returns>
        string GetPlaceInQueue(string queryStringPrefix);
        /// <summary>
        /// Returns the timestamp of when the Known User token was generated
        /// </summary>
        /// <param name="queryStringPrefix">The querystring prefix</param>
        /// <returns>The creation timestamp</returns>
        string GetTimeStamp(string queryStringPrefix);
        /// <summary>
        /// Returns the Event ID from the Known User token in the URL querystring
        /// </summary>
        /// <param name="queryStringPrefix">The querystring prefix</param>
        /// <returns>The Event ID</returns>
        string GetEventId(string queryStringPrefix);
        /// <summary>
        /// Returns the Customer ID from the Known User token in the URL querystring
        /// </summary>
        /// <param name="queryStringPrefix">The querystring prefix</param>
        /// <returns>The Customer ID</returns>
        string GetCustomerId(string queryStringPrefix);
        /// <summary>
        /// Returns the original target url without Known User token
        /// </summary>
        /// <param name="queryStringPrefix">The querystring prefix</param>
        /// <returns>The Original URL</returns>
        Uri GetOriginalUrl(string queryStringPrefix);
        /// <summary>
        /// Returns how the user has been redirected to the target URL
        /// </summary>
        /// <param name="queryStringPrefix">The querystring prefix</param>
        /// <returns>Method of redirect</returns>
        string GetRedirectType(string queryStringPrefix);
    }
}