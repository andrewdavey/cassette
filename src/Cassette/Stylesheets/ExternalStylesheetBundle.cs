using System;
using Cassette.Configuration;
using Cassette.Manifests;
using Cassette.Stylesheets.Manifests;

namespace Cassette.Stylesheets
{
    class ExternalStylesheetBundle : StylesheetBundle, IExternalBundle
    {
        public ExternalStylesheetBundle(string url)
            : base(url)
        {
            this.url = url;
        }

        public ExternalStylesheetBundle(string url, string applicationRelativePath) 
            : base(applicationRelativePath)
        {
            this.url = url;
        }

        readonly string url;
        ExternalStylesheetHtmlRenderer externalHtmlRender;

        internal string Url
        {
            get { return url; }
        }

        internal override void Process(CassetteSettings settings)
        {
            base.Process(settings);
            externalHtmlRender = new ExternalStylesheetHtmlRenderer(Renderer, settings);
        }

        internal override string Render()
        {
            return externalHtmlRender.Render(this);
        }

        internal override bool ContainsPath(string pathToFind)
        {
            return base.ContainsPath(pathToFind) || url.Equals(pathToFind, StringComparison.OrdinalIgnoreCase);
        }

        internal override BundleManifest CreateBundleManifest()
        {
            var builder = new ExternalStylesheetBundleManifestBuilder();
            return builder.BuildManifest(this);
        }

        internal override BundleManifest CreateBundleManifestIncludingContent()
        {
            var builder = new ExternalStylesheetBundleManifestBuilder { IncludeContent = true };
            return builder.BuildManifest(this);
        }

        string IExternalBundle.Url
        {
            get { return url; }
        }
    }
}