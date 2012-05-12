using System.Text;

namespace Cassette.Stylesheets
{
    class StylesheetHtmlRenderer : IBundleHtmlRenderer<StylesheetBundle>
    {
        public StylesheetHtmlRenderer(IUrlGenerator urlGenerator)
        {
            this.urlGenerator = urlGenerator;
        }

        readonly IUrlGenerator urlGenerator;

        public string Render(StylesheetBundle bundle)
        {
            var url = urlGenerator.CreateBundleUrl(bundle);
            var conditionalRenderer = new ConditionalRenderer();
            return conditionalRenderer.Render(bundle.Condition, html => RenderLink(bundle, html, url));
        }

        static StringBuilder RenderLink(StylesheetBundle bundle, StringBuilder html, string url)
        {
            return html.AppendFormat(
                HtmlConstants.LinkHtml,
                url,
                bundle.HtmlAttributes.CombinedAttributes
            );
        }
    }
}