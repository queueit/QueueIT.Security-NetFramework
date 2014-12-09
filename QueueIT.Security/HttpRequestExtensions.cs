using System;
using System.Web;

namespace QueueIT.Security
{
    /// <summary>
    /// Extensions to HttpRequest
    /// </summary>
    public static class HttpRequestExtensions
    {
        /// <summary>
        /// Gets the URL entered in the web browser
        /// </summary>
        /// <param name="request">The request</param>
        /// <returns>The browser URL</returns>
        public static string RealUrl(this HttpRequest request)
        {
            return string.Concat(
                request.Url.GetLeftPart(UriPartial.Authority),
                request.RawUrl);
        }
    }
}
