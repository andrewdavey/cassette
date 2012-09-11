using System;

namespace Cassette.HtmlAppCache
{
    public class FallbackMapping
    {
        readonly string urlNamespace;
        readonly string fallbackUrl;

        public FallbackMapping(string urlNamespace, string fallbackUrl)
        {
            if (string.IsNullOrWhiteSpace(urlNamespace)) throw new ArgumentException("urlNamespace cannot be null or whitespace.");
            if (string.IsNullOrWhiteSpace(fallbackUrl)) throw new ArgumentException("fallbackUrl cannot be null or whitespace.");
            
            this.urlNamespace = urlNamespace;
            this.fallbackUrl = fallbackUrl;
        }

        public string UrlNamespace
        {
            get { return urlNamespace; }
        }

        public string FallbackUrl
        {
            get { return fallbackUrl; }
        }

        public override string ToString()
        {
            return urlNamespace + " " + fallbackUrl;
        }
    }
}