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
            var html = new StringBuilder();
            var hasCondition = !string.IsNullOrEmpty(bundle.Condition);
            if (hasCondition)
            {
                html.AppendLine("<!--[if " + bundle.Condition + "]>");
            }

            if (string.IsNullOrEmpty(bundle.Media))
            {
                html.AppendFormat(
                    HtmlConstants.LinkHtml,
                    url,
                    bundle.HtmlAttributes.CombinedAttributes
                );
            }
            else
            {
                html.AppendFormat(
                    HtmlConstants.LinkWithMediaHtml,
                    url,
                    bundle.Media,
                    bundle.HtmlAttributes.CombinedAttributes
                );
            }

            if (hasCondition)
            {
                html.AppendLine();
                html.Append("<![endif]-->");
            }

            return html.ToString();
        }
    }
}