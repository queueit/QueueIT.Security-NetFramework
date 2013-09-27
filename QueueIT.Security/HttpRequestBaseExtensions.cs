using System;
using System.Web;

namespace QueueIT.Security
{
    /// <summary>
    /// Extensions to HttpRequestBase
    /// </summary>
    public static class HttpRequestBaseExtensions
    {
        /// <summary>
        /// Gets the URL entered in the web browser
        /// </summary>
        /// <param name="request">The request</param>
        /// <returns>The browser URL</returns>
        public static string RealUrl(this HttpRequestBase request)
        {
            return string.Concat(
                request.Url.GetLeftPart(UriPartial.Authority),
                request.RawUrl);
        }
    }
}