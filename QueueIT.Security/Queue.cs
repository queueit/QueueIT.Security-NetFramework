using System;
using System.Globalization;
using System.Text.RegularExpressions;
using System.Web;

namespace QueueIT.Security
{
    internal class Queue : IQueue
    {
        internal string DefaultDomainAlias { get; private set; }
        internal bool DefaultSslEnabled { get; private set; }
        internal bool DefaultIncludeTargetUrl { get; private set; }
        internal string DefaultLayoutName { get; private set; }
        internal CultureInfo DefaultLanguage { get; private set; }
        private Uri _defaultQueueUrl;
        private Uri _defaultLandingPageUrl;
        private Uri _defaultCancelUrl;

        public string EventId { get; private set; }
        public string CustomerId { get; private set; }

        internal Queue(string customerId, string eventId, string domainAlias, Uri landingPage, bool sslEnabled, bool includeTargetUrl, CultureInfo language, string layoutName)
        {
            this.CustomerId = customerId.ToLower();
            this.EventId = eventId.ToLower();
            this._defaultQueueUrl = GenerateQueueUrl(sslEnabled, domainAlias, language, layoutName).Uri;
            this._defaultCancelUrl = GenerateCancelUrl(sslEnabled, domainAlias).Uri;

            this.DefaultDomainAlias = domainAlias;
            this._defaultLandingPageUrl = landingPage;
            this.DefaultLanguage = language;
            this.DefaultLayoutName = layoutName;

            if (this._defaultLandingPageUrl != null && !this._defaultLandingPageUrl.IsAbsoluteUri &&
                HttpContext.Current != null)
            {
                Uri currentUrl = HttpContext.Current.Request.RealUrl();

                UriBuilder builder = new UriBuilder(currentUrl.Scheme, currentUrl.Host, currentUrl.Port, this._defaultLandingPageUrl.OriginalString);
                this._defaultLandingPageUrl = builder.Uri;
            }
            this.DefaultSslEnabled = sslEnabled;
            this.DefaultIncludeTargetUrl = includeTargetUrl;
        }

        public Uri GetQueueUrl(bool? includeTargetUrl = null, bool? sslEnabled = null, string domainAlias = null, CultureInfo language = null, string layoutName = null)
        {
            UriBuilder queueUrl = GetQueueUrlWithoutTarget(sslEnabled, domainAlias, language, layoutName);

            IncludeTargetUrl(includeTargetUrl, queueUrl);

            return queueUrl.Uri;
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

        public Uri GetQueueUrl(Uri targetUrl, bool? sslEnabled = null, string domainAlias = null, CultureInfo language = null, string layoutName = null)
        {
            UriBuilder queueUrl = GetQueueUrlWithoutTarget(sslEnabled, domainAlias, language, layoutName);

            IncludeTargetUrl(targetUrl, queueUrl);

            return queueUrl.Uri;
        }

        private void IncludeTargetUrl(bool? includeTargetUrl, UriBuilder queueUrl)
        {
            if (HttpContext.Current == null)
                return;

            if (!includeTargetUrl.HasValue)
                includeTargetUrl = this.DefaultIncludeTargetUrl;

            if (!includeTargetUrl.Value)
                return;

            IncludeTargetUrl(HttpContext.Current.Request.Url, queueUrl);
        }

        private static void IncludeTargetUrl(Uri targetUrl, UriBuilder queueUrl)
        {
            string query = (queueUrl.Query != null && queueUrl.Query.Length > 1) ? queueUrl.Query.Substring(1) : string.Empty;

            query = Regex.Replace(query, "(&?[tT]=[^&]*&?)", string.Empty);

            queueUrl.Query = string.Concat(
                query,
                string.IsNullOrEmpty(query) ? "t=" : "&t=", 
                HttpUtility.UrlEncode(targetUrl.AbsoluteUri));
        }

        public Uri GetCancelUrl(Uri landingPage = null, Guid? queueId = null, bool? sslEnabled = null, string domainAlias = null)
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
            if (landingPage != null)
                cancelUrl.Query = string.Concat(
                    (cancelUrl.Query != null && cancelUrl.Query.Length > 1) ? cancelUrl.Query.Substring(1) : string.Empty,
                    "&r=", 
                    HttpUtility.UrlEncode(landingPage.AbsoluteUri));
            if (landingPage == null && _defaultLandingPageUrl != null)
                cancelUrl.Query = string.Concat(
                    (cancelUrl.Query != null && cancelUrl.Query.Length > 1) ? cancelUrl.Query.Substring(1) : string.Empty,
                    "&r=", 
                    HttpUtility.UrlEncode(_defaultLandingPageUrl.AbsoluteUri));

            return cancelUrl.Uri;
        }

        public Uri GetLandingPageUrl(bool? includeTargetUrl)
        {
            if (this._defaultLandingPageUrl == null)
                return null;

            if ((!includeTargetUrl.HasValue || !includeTargetUrl.Value) && !this.DefaultIncludeTargetUrl)
                return this._defaultLandingPageUrl;

            UriBuilder builder = new UriBuilder(this._defaultLandingPageUrl);

            IncludeTargetUrl(includeTargetUrl, builder);

            return builder.Uri;
        }

        public Uri GetLandingPageUrl(Uri targetUrl)
        {
            if (this._defaultLandingPageUrl == null)
                return null;

            UriBuilder builder = new UriBuilder(this._defaultLandingPageUrl);

            IncludeTargetUrl(targetUrl, builder);

            return builder.Uri;
        }

        private UriBuilder GenerateQueueUrl(bool? sslEnabled, string domainAlias, CultureInfo language, string layoutName)
        {
            if (string.IsNullOrEmpty(domainAlias))
                domainAlias = DefaultDomainAlias;

            UriBuilder uri = new UriBuilder(
                sslEnabled.HasValue && sslEnabled.Value ? "https" : "http",
                domainAlias);

            uri.Query = string.Format(
                "c={0}&e={1}",
                this.CustomerId,
                this.EventId);

            if (language != null)
                uri.Query = string.Concat(uri.Query.Substring(1), "&cid=", language.Name);
            if (layoutName != null)
                uri.Query = string.Concat(uri.Query.Substring(1), "&l=", HttpUtility.UrlEncode(layoutName));

            return uri;
        }

        private UriBuilder GenerateCancelUrl(bool? sslEnabled, string domainAlias)
        {
            if (string.IsNullOrEmpty(domainAlias))
                domainAlias = DefaultDomainAlias;

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
