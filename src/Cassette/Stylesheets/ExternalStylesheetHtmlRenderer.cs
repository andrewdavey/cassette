using System;
using System.Linq;

namespace Cassette.Stylesheets
{
    class ExternalStylesheetHtmlRenderer : IBundleHtmlRenderer<ExternalStylesheetBundle>
    {
        readonly IBundleHtmlRenderer<StylesheetBundle> fallbackRenderer;
        readonly ICassetteApplication application;

        public ExternalStylesheetHtmlRenderer(
            IBundleHtmlRenderer<StylesheetBundle> fallbackRenderer,
            ICassetteApplication application)
        {
            this.fallbackRenderer = fallbackRenderer;
            this.application = application;
        }

        public string Render(ExternalStylesheetBundle bundle)
        {
            if (application.IsDebuggingEnabled && bundle.Assets.Any())
            {
                return fallbackRenderer.Render(bundle);
            }
            else
            {
                if (string.IsNullOrEmpty(bundle.Media))
                {
                    return string.Format(HtmlConstants.LinkHtml, bundle.Url);
                }
                else
                {
                    return string.Format(HtmlConstants.LinkWithMediaHtml, bundle.Url, bundle.Media);
                }
            }
        }
    }
}