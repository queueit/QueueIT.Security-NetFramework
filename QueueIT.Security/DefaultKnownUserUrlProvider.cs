using System;
using System.Collections.Specialized;
using System.Text;
using System.Web;

namespace QueueIT.Security
{
    /// <summary>
    /// The default Known User Url Provider which gets the URL and Known User tokens from the request object. 
    /// Override this if your application does URL rewrite to make the URL the same as the configured target URL on the event.
    /// </summary>
    public class DefaultKnownUserUrlProvider : IKnownUserUrlProvider
    {
        private HttpRequest _request;

        public DefaultKnownUserUrlProvider()
        {
            if (HttpContext.Current == null)
                throw new ApplicationException("Current HTTP context is null");

            this._request = HttpContext.Current.Request;
        }

        public DefaultKnownUserUrlProvider(HttpRequest request)
        {
            if (request == null)
                throw new ApplicationException("HTTP request is null");

            this._request = request;
        }

        /// <summary>
        /// Returns the target URL as configured on event with Known User tokens
        /// </summary>
        /// <returns>The queue target url</returns>
        public virtual Uri GetUrl()
        {
            return this._request.RealUrl();
        }

        /// <summary>
        /// The queue ID of the Known User token
        /// </summary>
        /// <remarks>The 'q' querystring paramenter</remarks>
        /// <param name="queryStringPrefix">Optional query string prefix as configured on the queue event</param>
        /// <returns>The queue ID</returns>
        public virtual string GetQueueId(string queryStringPrefix)
        {
            string queueIdParameter = this._request.QueryString[string.Concat(queryStringPrefix, "q")];
            if (string.IsNullOrEmpty(queueIdParameter))
                return null;

            return queueIdParameter;
        }

        /// <summary>
        /// Returns the obfuscated queue number
        /// </summary>
        /// <remarks>The 'p' querystring parameter</remarks>
        /// <param name="queryStringPrefix">Optional query string prefix as configured on the queue event</param>
        /// <returns>The obfuscated queue number</returns>
        public virtual string GetPlaceInQueue(string queryStringPrefix)
        {
            string placeInQueueParameter = this._request.QueryString[string.Concat(queryStringPrefix, "p")];
            if (string.IsNullOrEmpty(placeInQueueParameter))
                return null;

            return placeInQueueParameter;
        }

        /// <summary>
        /// Returns the time stamp as seconds since 1970 (unix time)
        /// </summary>
        /// <remarks>The 'ts' querystring parameter</remarks>
        /// <param name="queryStringPrefix">Optional query string prefix as configured on the queue event</param>
        /// <returns>The time stamp as seconds since 1970</returns>
        public virtual string GetTimeStamp(string queryStringPrefix)
        {
            string timestampParameter = this._request.QueryString[string.Concat(queryStringPrefix, "ts")];

            if (string.IsNullOrEmpty(timestampParameter))
                return null;

            return timestampParameter;
        }

        /// <summary>
        /// Returns the event ID
        /// </summary>
        /// <remarks>The 'e' querystring parameter</remarks>
        /// <param name="queryStringPrefix">Optional query string prefix as configured on the queue event</param>
        /// <returns>The event ID</returns>
        public virtual string GetEventId(string queryStringPrefix)
        {
            string eventIdParameter = this._request.QueryString[string.Concat(queryStringPrefix, "e")];

            if (string.IsNullOrEmpty(eventIdParameter))
                return null;

            return eventIdParameter;
        }

        /// <summary>
        /// Returns the customer ID
        /// </summary>
        /// <remarks>The 'c' querystring parameter</remarks>
        /// <param name="queryStringPrefix">Optional query string prefix as configured on the queue event</param>
        /// <returns>The customer ID</returns>
        public virtual string GetCustomerId(string queryStringPrefix)
        {
            string customerIdParameter = this._request.QueryString[string.Concat(queryStringPrefix, "c")];

            if (string.IsNullOrEmpty(customerIdParameter))
                return null;

            return customerIdParameter;
        }

        /// <summary>
        /// Returns the redirect type
        /// </summary>
        /// <remarks>The 'rt' querystring parameter</remarks>
        /// <param name="queryStringPrefix">Optional query string prefix as configured on the queue event</param>
        /// <returns>The redirect type</returns>
        public virtual string GetRedirectType(string queryStringPrefix)
        {
            string redirectTypeParameter = this._request.QueryString[string.Concat(queryStringPrefix, "rt")];

            if (string.IsNullOrEmpty(redirectTypeParameter))
                return null;

            return redirectTypeParameter;
        }

        /// <summary>
        /// Returns the target URL without Known User tokens
        /// </summary>
        /// <param name="queryStringPrefix">Optional query string prefix as configured on the queue event</param>
        /// <returns>The target URL without Known User tokens</returns>
        public virtual Uri GetOriginalUrl(string queryStringPrefix)
        {
            UriBuilder uriBuilder = new UriBuilder(this.GetUrl());
            NameValueCollection querystringParameters = HttpUtility.ParseQueryString(uriBuilder.Query);
            querystringParameters.Remove(queryStringPrefix + "q");
            querystringParameters.Remove(queryStringPrefix + "h");
            querystringParameters.Remove(queryStringPrefix + "p");
            querystringParameters.Remove(queryStringPrefix + "ts");
            querystringParameters.Remove(queryStringPrefix + "e");
            querystringParameters.Remove(queryStringPrefix + "c");
            querystringParameters.Remove(queryStringPrefix + "rt");

            StringBuilder sb = new StringBuilder();
            foreach (string querystringParameter in querystringParameters.AllKeys)
            {
                foreach (var parameterValue in querystringParameters.GetValues(querystringParameter))
                {
                    if (sb.Length > 0)
                        sb.Append("&");
                    sb.Append(querystringParameter);
                    sb.Append("=");
                    sb.Append(HttpUtility.UrlEncode(parameterValue));
                }
            }

            uriBuilder.Query = sb.ToString();

            return uriBuilder.Uri;
        }
    }
}
