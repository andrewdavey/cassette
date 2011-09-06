using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Cassette.Stylesheets
{
    public class DebugStylesheetHtmlRenderer : IModuleHtmlRenderer<StylesheetModule>
    {
        public DebugStylesheetHtmlRenderer(IUrlGenerator urlGenerator)
        {
            this.urlGenerator = urlGenerator;
        }

        readonly IUrlGenerator urlGenerator;

        public IHtmlString Render(StylesheetModule module)
        {
            var assetUrls = GetAssetUrls(module);
            var createLink = GetCreateLinkFunc(module);

            return new HtmlString(
                string.Join(
                    Environment.NewLine,
                    assetUrls.Select(createLink)
                )
            );
        }

        IEnumerable<string> GetAssetUrls(StylesheetModule module)
        {
            return module.Assets.Select(
                asset => asset.HasTransformers 
                    ? urlGenerator.CreateAssetCompileUrl(module, asset)
                    : urlGenerator.CreateAssetUrl(asset)
            );
        }

        Func<string, string> GetCreateLinkFunc(StylesheetModule module)
        {
            if (string.IsNullOrEmpty(module.Media))
            {
                return url => string.Format(
                    HtmlConstants.LinkHtml,
                    url
                );
            }
            else
            {
                return url => string.Format(
                    HtmlConstants.LinkWithMediaHtml,
                    url,
                    module.Media
                );
            }
        }
    }
}