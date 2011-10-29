using System;
using System.Linq;
using System.Web;

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

        public IHtmlString Render(ExternalStylesheetBundle bundle)
        {
            if (application.IsDebuggingEnabled && bundle.Assets.Any())
            {
                return fallbackRenderer.Render(bundle);
            }
            else
            {
                if (string.IsNullOrEmpty(bundle.Media))
                {
                    return new HtmlString(String.Format(HtmlConstants.LinkHtml, bundle.Url));
                }
                else
                {
                    return new HtmlString(String.Format(HtmlConstants.LinkWithMediaHtml, bundle.Url, bundle.Media));
                }
            }
        }
    }
}