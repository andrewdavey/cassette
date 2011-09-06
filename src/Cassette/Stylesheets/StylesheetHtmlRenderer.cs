using System.Web;

namespace Cassette.Stylesheets
{
    public class StylesheetHtmlRenderer : IModuleHtmlRenderer<StylesheetModule>
    {
        public StylesheetHtmlRenderer(IUrlGenerator urlGenerator)
        {
            this.urlGenerator = urlGenerator;
        }

        readonly IUrlGenerator urlGenerator;

        public IHtmlString Render(StylesheetModule module)
        {
            var url = urlGenerator.CreateModuleUrl(module);
            if (string.IsNullOrEmpty(module.Media))
            {
                return new HtmlString(
                    string.Format(
                        HtmlConstants.LinkHtml,
                        url
                    )
                );
            }
            else
            {
                return new HtmlString(
                    string.Format(
                        HtmlConstants.LinkWithMediaHtml,
                        url,
                        module.Media
                    )
                );
            }
        }
    }
}
