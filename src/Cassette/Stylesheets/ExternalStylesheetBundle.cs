using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml.Linq;

namespace Cassette.Stylesheets
{
#pragma warning disable 659
    class ExternalStylesheetBundle : StylesheetBundle, IExternalBundle, IBundleHtmlRenderer<StylesheetBundle>
    {
        readonly string url;
        bool isDebuggingEnabled;
        IBundleHtmlRenderer<StylesheetBundle> fallbackRenderer;

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
            fallbackRenderer = Renderer;
            isDebuggingEnabled = settings.IsDebuggingEnabled;
            Renderer = this;
        }

        internal override bool ContainsPath(string pathToFind)
        {
            return base.ContainsPath(pathToFind) || url.Equals(pathToFind, StringComparison.OrdinalIgnoreCase);
        }

        internal override IEnumerable<string> GetUrls(bool isDebuggingEnabled, IUrlGenerator urlGenerator)
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

        public string ExternalUrl
        {
            get { return url; }
        }

        public string Render(StylesheetBundle unusedParameter)
        {
            if (isDebuggingEnabled && Assets.Any())
            {
                return fallbackRenderer.Render(this);
            }

            var conditionalRenderer = new ConditionalRenderer();

            return conditionalRenderer.Render(Condition, RenderLink);
        }

        void RenderLink(StringBuilder html)
        {
            html.AppendFormat(
                HtmlConstants.LinkHtml,
                url,
                HtmlAttributes.CombinedAttributes
                );
        }

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
#pragma warning restore 659
    }
}