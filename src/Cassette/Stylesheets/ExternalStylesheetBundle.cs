using System;
using System.Linq;
using System.Text;
using Cassette.Configuration;
using Cassette.Manifests;
using Cassette.Stylesheets.Manifests;

namespace Cassette.Stylesheets
{
    [ProtoBuf.ProtoContract]
    class ExternalStylesheetBundle : StylesheetBundle, IExternalBundle, IBundleHtmlRenderer<StylesheetBundle>
    {
        [ProtoBuf.ProtoMember(1)]
        readonly string url;
        [ProtoBuf.ProtoMember(2)]
        bool isDebuggingEnabled;

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
            isDebuggingEnabled = settings.IsDebuggingEnabled;
            Renderer = this;
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

        public string ExternalUrl
        {
            get { return url; }
        }

        internal IBundleHtmlRenderer<StylesheetBundle> FallbackRenderer { get; set; } 

        public string Render(StylesheetBundle unusedParameter)
        {
            if (isDebuggingEnabled && Assets.Any())
            {
                return FallbackRenderer.Render(this);
            }

            var conditionalRenderer = new ConditionalRenderer();

            return conditionalRenderer.Render(Condition, html =>
            {
                if (string.IsNullOrEmpty(Media))
                {
                    RenderLink(html);
                }
                else
                {
                    RenderLinkWithMedia(html);
                }
            });
        }

        void RenderLink(StringBuilder html)
        {
            html.AppendFormat(
                HtmlConstants.LinkHtml,
                url,
                HtmlAttributes.CombinedAttributes
                );
        }

        void RenderLinkWithMedia(StringBuilder html)
        {
            html.AppendFormat(
                HtmlConstants.LinkWithMediaHtml,
                url,
                Media,
                HtmlAttributes.CombinedAttributes
                );
        }
    }
}