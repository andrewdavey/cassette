﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml.Linq;

namespace Cassette.Stylesheets
{
#pragma warning disable 659
    public class ExternalStylesheetBundle : StylesheetBundle, IExternalBundle
    {
        readonly string url;

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

        protected override void ProcessCore(CassetteSettings settings)
        {
            base.ProcessCore(settings);
            FallbackRenderer = Renderer;
            Renderer = new ExternalStylesheetBundleRenderer(settings);
        }

        internal override bool ContainsPath(string pathToFind)
        {
            return base.ContainsPath(pathToFind) || url.Equals(pathToFind, StringComparison.OrdinalIgnoreCase);
        }

        public override IEnumerable<string> GetUrls(bool isDebuggingEnabled, IUrlGenerator urlGenerator)
        {
            if (isDebuggingEnabled && Assets.Any())
            {
                return base.GetUrls(true, urlGenerator);
            }
            else
            {
                return new[] { ExternalUrl };
            }
        }

        public virtual string ExternalUrl
        {
            get { return url; }
        }

        public IBundleHtmlRenderer<StylesheetBundle> FallbackRenderer { get; set; }

        internal override void SerializeInto(XContainer container)
        {
            var serializer = new ExternalStylesheetBundleSerializer(container);
            serializer.Serialize(this);
        }

        public override bool Equals(object obj)
        {
            var other = obj as ExternalStylesheetBundle;
            return base.Equals(obj)
                   && other != null
                   && other.url == url;
        }

        public class ExternalStylesheetBundleRenderer : IBundleHtmlRenderer<StylesheetBundle>
        {
            readonly CassetteSettings settings;

            public ExternalStylesheetBundleRenderer(CassetteSettings settings)
            {
                this.settings = settings;
            }

            public string Render(StylesheetBundle bundle)
            {
                return Render((ExternalStylesheetBundle)bundle);
            }

            public string Render(ExternalStylesheetBundle bundle)
            {
                if (settings.IsDebuggingEnabled && bundle.Assets.Any())
                {
                    return bundle.FallbackRenderer.Render(bundle);
                }

                var conditionalRenderer = new ConditionalRenderer();
                return conditionalRenderer.Render(bundle.Condition, html => RenderLink(html, bundle));
            }

            void RenderLink(StringBuilder html, ExternalStylesheetBundle bundle)
            {
                html.AppendFormat(
                    HtmlConstants.LinkHtml,
                    bundle.ExternalUrl,
                    bundle.HtmlAttributes.CombinedAttributes
                );
            }
        }

#pragma warning restore 659
    }
}