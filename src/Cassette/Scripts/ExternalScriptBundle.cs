using System;
using Cassette.Configuration;

namespace Cassette.Scripts
{
    class ExternalScriptBundle : ScriptBundle
    {
        readonly string url;
        readonly string fallbackCondition;
        ExternalScriptBundleHtmlRenderer externalRenderer;

        public ExternalScriptBundle(string url)
            : base(url)
        {
            ValidateUrl(url);
            this.url = url;
        }

        public ExternalScriptBundle(string url, string bundlePath, string fallbackCondition = null)
            : base(bundlePath)
        {
            ValidateUrl(url);
            this.url = url;
            this.fallbackCondition = fallbackCondition;
        }

        static void ValidateUrl(string url)
        {
            if (url == null) throw new ArgumentNullException("url");
            if (string.IsNullOrWhiteSpace(url)) throw new ArgumentException("URL is required.", "url");
        }

        internal string Url
        {
            get { return url; }
        }

        internal string FallbackCondition
        {
            get { return fallbackCondition; }
        }

        internal override void Process(CassetteSettings settings)
        {
            // Any fallback assets are processed like a regular ScriptBundle.
            base.Process(settings);
            // We just need a special renderer instead.
            externalRenderer = new ExternalScriptBundleHtmlRenderer(Renderer, settings);
        }

        internal override string Render()
        {
            return externalRenderer.Render(this);
        }

        internal override bool ContainsPath(string pathToFind)
        {
            return base.ContainsPath(pathToFind) || url.Equals(pathToFind, StringComparison.OrdinalIgnoreCase);
        }
    }
}