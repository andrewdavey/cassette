using System.Linq;
using Cassette.Configuration;
using Cassette.Utilities;

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
            else
            {
                if (string.IsNullOrEmpty(bundle.Media))
                {
                    return string.Format(
                        HtmlConstants.LinkHtml,
                        bundle.Url,
                        bundle.HtmlAttributes.CombinedAttributes
                    );
                }
                else
                {
                    return string.Format(
                        HtmlConstants.LinkWithMediaHtml, 
                        bundle.Url, 
                        bundle.Media,
                        bundle.HtmlAttributes.CombinedAttributes
                    );
                }
            }
        }
    }
}
