﻿using System;
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

        internal override string Url
        {
            get { return url; }
        }

        protected override void ProcessCore(CassetteSettings settings)
        {
            base.ProcessCore(settings);
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

        internal override BundleManifest CreateBundleManifest(bool includeProcessedBundleContent)
        {
            var builder = new ExternalStylesheetBundleManifestBuilder { IncludeContent = includeProcessedBundleContent };
            return builder.BuildManifest(this);
        }

        string IExternalBundle.Url
        {
            get { return url; }
        }
    }
}