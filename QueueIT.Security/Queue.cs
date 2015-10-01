using System;
using System.Globalization;
using System.Text.RegularExpressions;
using System.Web;

namespace QueueIT.Security
{
    internal class Queue : IQueue
    {
        public string DomainAlias { get; private set; }
        public bool SslEnabled { get; private set; }
        public bool IncludeTargetUrl { get; private set; }
        public string LayoutName { get; private set; }
        public CultureInfo Language { get; private set; }

        private string _defaultQueueUrl;
        private string _defaultLandingPageUrl;
        private string _defaultCancelUrl;

        public string EventId { get; private set; }
        public string CustomerId { get; private set; }

        internal Queue(string customerId, string eventId, string domainAlias, string landingPage, bool sslEnabled, bool includeTargetUrl, CultureInfo language, string layoutName)
        {
            this.CustomerId = customerId.ToLower();
            this.EventId = eventId.ToLower();
            this._defaultQueueUrl = GenerateQueueUrl(sslEnabled, domainAlias, language, layoutName).Uri.AbsoluteUri;
            this._defaultCancelUrl = GenerateCancelUrl(sslEnabled, domainAlias).Uri.AbsoluteUri;

            this.DomainAlias = domainAlias;
            this._defaultLandingPageUrl = landingPage;
            this.Language = language;
            this.LayoutName = layoutName;

            if (!string.IsNullOrEmpty(this._defaultLandingPageUrl) && !this._defaultLandingPageUrl.StartsWith("http") &&
                HttpContext.Current != null)
            {
                UriBuilder builder = new UriBuilder(
                    HttpContext.Current.Request.Url.Scheme, 
                    HttpContext.Current.Request.Url.Host, 
                    HttpContext.Current.Request.Url.Port, 
                    this._defaultLandingPageUrl);
                this._defaultLandingPageUrl = builder.Uri.AbsoluteUri;
            }
            this.SslEnabled = sslEnabled;
            this.IncludeTargetUrl = includeTargetUrl;
        }

        public string GetQueueUrl(bool? includeTargetUrl = null, bool? sslEnabled = null, string domainAlias = null, CultureInfo language = null, string layoutName = null)
        {
            UriBuilder queueUrl = GetQueueUrlWithoutTarget(sslEnabled, domainAlias, language, layoutName);

            AddTargetUrl(includeTargetUrl, queueUrl);

            return queueUrl.Uri.AbsoluteUri;
        }

        private UriBuilder GetQueueUrlWithoutTarget(bool? sslEnabled, string domainAlias, CultureInfo language, string layoutName)
        {
            UriBuilder queueUrl = domainAlias != null || language != null || layoutName != null
                ? GenerateQueueUrl(sslEnabled, domainAlias, language, layoutName)
                : new UriBuilder(this._defaultQueueUrl);

            if (sslEnabled.HasValue)
            {
                queueUrl.Scheme = sslEnabled.Value ? "https" : "http";
                queueUrl.Port = sslEnabled.Value ? 443 : 80;
            }

            return queueUrl;
        }

        public string GetQueueUrl(string targetUrl, bool? sslEnabled = null, string domainAlias = null, CultureInfo language = null, string layoutName = null)
        {
            UriBuilder queueUrl = GetQueueUrlWithoutTarget(sslEnabled, domainAlias, language, layoutName);

            AddTargetUrl(targetUrl, queueUrl);

            return queueUrl.Uri.AbsoluteUri;
        }

        private void AddTargetUrl(bool? includeTargetUrl, UriBuilder queueUrl)
        {
            if (HttpContext.Current == null)
                return;

            if (!includeTargetUrl.HasValue)
                includeTargetUrl = this.IncludeTargetUrl;

            if (!includeTargetUrl.Value)
                return;
            
            AddTargetUrl(HttpContext.Current.Request.RealUrl(), queueUrl);
        }

        private static void AddTargetUrl(string targetUrl, UriBuilder queueUrl)
        {
            string query = (queueUrl.Query != null && queueUrl.Query.Length > 1) ? queueUrl.Query.Substring(1) : string.Empty;

            query = Regex.Replace(query, "(&?[tT]=[^&]*&?)", string.Empty);

            queueUrl.Query = string.Concat(
                query,
                string.IsNullOrEmpty(query) ? "t=" : "&t=", 
                HttpUtility.UrlEncode(targetUrl));
        }

        public string GetCancelUrl(string landingPageUrl = null, Guid? queueId = null, bool? sslEnabled = null, string domainAlias = null)
        {
            UriBuilder cancelUrl = domainAlias != null
                ? GenerateCancelUrl(sslEnabled, domainAlias)
                : new UriBuilder(this._defaultCancelUrl);

            if (sslEnabled.HasValue)
            {
                cancelUrl.Scheme = sslEnabled.Value ? "https" : "http";
                cancelUrl.Port = sslEnabled.Value ? 443 : 80;
            }

            if (queueId.HasValue)
                cancelUrl.Query = string.Concat(
                    (cancelUrl.Query != null && cancelUrl.Query.Length > 1) ? cancelUrl.Query.Substring(1) : string.Empty, 
                    "&q=", 
                    queueId.Value.ToString());
            if (landingPageUrl != null)
                cancelUrl.Query = string.Concat(
                    (cancelUrl.Query != null && cancelUrl.Query.Length > 1) ? cancelUrl.Query.Substring(1) : string.Empty,
                    "&r=",
                    HttpUtility.UrlEncode(landingPageUrl));
            if (landingPageUrl == null && _defaultLandingPageUrl != null)
                cancelUrl.Query = string.Concat(
                    (cancelUrl.Query != null && cancelUrl.Query.Length > 1) ? cancelUrl.Query.Substring(1) : string.Empty,
                    "&r=", 
                    HttpUtility.UrlEncode(_defaultLandingPageUrl));

            return cancelUrl.Uri.AbsoluteUri;
        }

        public string GetLandingPageUrl(bool? includeTargetUrl)
        {
            if (this._defaultLandingPageUrl == null)
                return null;

            if ((!includeTargetUrl.HasValue || !includeTargetUrl.Value) && !this.IncludeTargetUrl)
                return this._defaultLandingPageUrl;

            UriBuilder builder = new UriBuilder(this._defaultLandingPageUrl);

            AddTargetUrl(includeTargetUrl, builder);

            return builder.Uri.AbsoluteUri;
        }

        public string GetLandingPageUrl(string targetUrl)
        {
            if (this._defaultLandingPageUrl == null)
                return null;

            UriBuilder builder = new UriBuilder(this._defaultLandingPageUrl);

            AddTargetUrl(targetUrl, builder);

            return builder.Uri.AbsoluteUri;
        }

        private UriBuilder GenerateQueueUrl(bool? sslEnabled, string domainAlias, CultureInfo language, string layoutName)
        {
            if (string.IsNullOrEmpty(domainAlias))
                domainAlias = DomainAlias;

            UriBuilder uri = new UriBuilder(
                sslEnabled.HasValue && sslEnabled.Value ? "https" : "http",
                domainAlias);

            uri.Query = string.Format(
                "c={0}&e={1}&ver=c{2}",
                this.CustomerId,
                this.EventId,
                this.GetType().Assembly.GetName().Version.ToString());

            if (language != null)
                uri.Query = string.Concat(uri.Query.Substring(1), "&cid=", language.Name);
            if (layoutName != null)
                uri.Query = string.Concat(uri.Query.Substring(1), "&l=", HttpUtility.UrlEncode(layoutName));

            return uri;
        }

        private UriBuilder GenerateCancelUrl(bool? sslEnabled, string domainAlias)
        {
            if (string.IsNullOrEmpty(domainAlias))
                domainAlias = DomainAlias;

            UriBuilder uri = new UriBuilder(
                sslEnabled.HasValue && sslEnabled.Value ? "https" : "http",
                domainAlias);

            uri.Path = "cancel.aspx";
            uri.Query = string.Format("c={0}&e={1}",
                this.CustomerId,
                this.EventId);

            return uri;
        }
    }
}
