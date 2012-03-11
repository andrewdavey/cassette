using System.Linq;
using System.Text;
using Cassette.Configuration;

namespace Cassette.Stylesheets
{
    class ExternalStylesheetHtmlRenderer : IBundleHtmlRenderer<StylesheetBundle>
    {
        readonly IBundleHtmlRenderer<StylesheetBundle> fallbackRenderer;
        readonly CassetteSettings settings;

        public ExternalStylesheetHtmlRenderer(IBundleHtmlRenderer<StylesheetBundle> fallbackRenderer, CassetteSettings settings)
        {
            this.fallbackRenderer = fallbackRenderer;
            this.settings = settings;
        }

        public string Render(StylesheetBundle bundle)
        {
            if (settings.IsDebuggingEnabled && bundle.Assets.Any())
            {
                return fallbackRenderer.Render(bundle);
            }

            var html = new StringBuilder();

            if (string.IsNullOrEmpty(bundle.Media))
            {
                html.AppendFormat(
                    HtmlConstants.LinkHtml, 
                    bundle.Url, 
                    bundle.HtmlAttributes.CombinedAttributes
                );
            }
            else
            {
                html.AppendFormat(
                    HtmlConstants.LinkWithMediaHtml,
                    bundle.Url,
                    bundle.Media,
                    bundle.HtmlAttributes.CombinedAttributes
                );
            }

            if (bundle.HasCondition)
            {
                return new ConditionalRenderer().RenderCondition(bundle.Condition, html.ToString());
            }
            else
            {
                return html.ToString();
            }
        }
    }
}