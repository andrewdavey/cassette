using System;
using System.Linq;
using System.Text;
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
        CassetteSettings settings;

        protected override void ProcessCore(CassetteSettings settings)
        {
            base.ProcessCore(settings);
            this.settings = settings;
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

        internal override string Render()
        {
            if (settings.IsDebuggingEnabled && Assets.Any())
            {
                return base.Render();
            }

            var html = new StringBuilder();

            var hasCondition = !string.IsNullOrEmpty(Condition);
            RenderConditionalCommentStart(html, hasCondition);
            if (string.IsNullOrEmpty(Media))
            {
                RenderLink(html);
            }
            else
            {
                RenderLinkWithMedia(html);
            }
            RenderConditionalCommentEnd(html, hasCondition);

            return html.ToString();
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

        void RenderConditionalCommentStart(StringBuilder html, bool hasCondition)
        {
            if (hasCondition)
            {
                html.AppendFormat(HtmlConstants.ConditionalCommentStart, Condition);
                html.AppendLine();
            }
        }

        void RenderConditionalCommentEnd(StringBuilder html, bool hasCondition)
        {
            if (hasCondition)
            {
                html.AppendLine();
                html.Append(HtmlConstants.ConditionalCommentEnd);
            }
        }
    }
}