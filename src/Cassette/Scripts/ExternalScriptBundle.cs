using System;
using Cassette.Configuration;
using Cassette.Manifests;
using Cassette.Scripts.Manifests;

namespace Cassette.Scripts
{
    class ExternalScriptBundle : ScriptBundle, IExternalBundle
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

        internal override BundleManifest CreateBundleManifest()
        {
            var builder = new ExternalScriptBundleManifestBuilder();
            return builder.BuildManifest(this);
        }

        internal override BundleManifest CreateBundleManifestIncludingContent()
        {
            var builder = new ExternalScriptBundleManifestBuilder { IncludeContent = true };
            return builder.BuildManifest(this);
        }

        string IExternalBundle.Url
        {
            get { return url; }
        }
    }
}