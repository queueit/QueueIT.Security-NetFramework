using System;
using System.Collections.Specialized;
using System.Text;
using System.Web;

namespace QueueIT.Security
{
    internal class DefaultKnownUserUrlProvider : IKnownUserUrlProvider
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

        public Uri GetUrl()
        {
            return this._request.RealUrl();
        }

        public string GetQueueId(string queryStringPrefix)
        {
            string queueIdParameter = this._request.QueryString[string.Concat(queryStringPrefix, "q")];
            if (string.IsNullOrEmpty(queueIdParameter))
                return null;

            return queueIdParameter;
        }

        public string GetPlaceInQueue(string queryStringPrefix)
        {
            string placeInQueueParameter = this._request.QueryString[string.Concat(queryStringPrefix, "p")];
            if (string.IsNullOrEmpty(placeInQueueParameter))
                return null;

            return placeInQueueParameter;
        }

        public string GetTimeStamp(string queryStringPrefix)
        {
            string timestampParameter = this._request.QueryString[string.Concat(queryStringPrefix, "ts")];

            if (string.IsNullOrEmpty(timestampParameter))
                return null;

            return timestampParameter;
        }

        public string GetEventId(string queryStringPrefix)
        {
            string eventIdParameter = this._request.QueryString[string.Concat(queryStringPrefix, "e")];

            if (string.IsNullOrEmpty(eventIdParameter))
                return null;

            return eventIdParameter;
        }

        public string GetCustomerId(string queryStringPrefix)
        {
            string customerIdParameter = this._request.QueryString[string.Concat(queryStringPrefix, "c")];

            if (string.IsNullOrEmpty(customerIdParameter))
                return null;

            return customerIdParameter;
        }

        public string GetRedirectType(string queryStringPrefix)
        {
            string redirectTypeParameter = this._request.QueryString[string.Concat(queryStringPrefix, "rt")];

            if (string.IsNullOrEmpty(redirectTypeParameter))
                return null;

            return redirectTypeParameter;
        }

        public Uri GetOriginalUrl(string queryStringPrefix)
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
