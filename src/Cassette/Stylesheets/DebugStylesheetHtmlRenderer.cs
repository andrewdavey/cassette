using System;
using System.Collections.Generic;
using System.Linq;

namespace Cassette.Stylesheets
{
    class DebugStylesheetHtmlRenderer : IBundleHtmlRenderer<StylesheetBundle>
    {
        public DebugStylesheetHtmlRenderer(IUrlGenerator urlGenerator)
        {
            this.urlGenerator = urlGenerator;
        }

        readonly IUrlGenerator urlGenerator;

        public string Render(StylesheetBundle bundle)
        {
            var assetUrls = GetAssetUrls(bundle);
            var createLink = GetCreateLinkFunc(bundle);
            var content = string.Join(
                Environment.NewLine,
                assetUrls.Select(createLink).ToArray()
            );

            var conditionalRenderer = new ConditionalRenderer();
            return conditionalRenderer.Render(bundle.Condition, html => html.Append(content));
        }

        IEnumerable<string> GetAssetUrls(StylesheetBundle bundle)
        {
            return bundle.Assets.Select(urlGenerator.CreateAssetUrl);
        }

        Func<string, string> GetCreateLinkFunc(StylesheetBundle bundle)
        {
            return url => string.Format(
                HtmlConstants.LinkHtml,
                url,
                bundle.HtmlAttributes.CombinedAttributes
            );
        }
    }
}
