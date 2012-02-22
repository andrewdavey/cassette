using System.Linq;
using System.Text;
using Cassette.Configuration;

namespace Cassette.Stylesheets
{
    class ExternalStylesheetHtmlRenderer : IBundleHtmlRenderer<ExternalStylesheetBundle>
    {
        readonly IBundleHtmlRenderer<StylesheetBundle> fallbackRenderer;
        readonly CassetteSettings settings;

        public ExternalStylesheetHtmlRenderer(IBundleHtmlRenderer<StylesheetBundle> fallbackRenderer, CassetteSettings settings)
        {
            this.fallbackRenderer = fallbackRenderer;
            this.settings = settings;
        }

        public string Render(ExternalStylesheetBundle bundle)
        {
            if (settings.IsDebuggingEnabled && bundle.Assets.Any())
            {
                return fallbackRenderer.Render(bundle);
            }

            var html = new StringBuilder();

            var hasCondition = !string.IsNullOrEmpty(bundle.Condition);
            if (hasCondition)
            {
                html.AppendFormat(HtmlConstants.ConditionalCommentStart, bundle.Condition);
                html.AppendLine();
            }
            
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
            
            if (hasCondition)
            {
                html.AppendLine();
                html.Append(HtmlConstants.ConditionalCommentEnd);
            }

            return html.ToString();
        }
    }
}