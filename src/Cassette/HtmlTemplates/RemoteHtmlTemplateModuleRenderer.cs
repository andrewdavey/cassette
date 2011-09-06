using System.Web;

namespace Cassette.HtmlTemplates
{
    public class RemoteHtmlTemplateModuleRenderer : IModuleHtmlRenderer<HtmlTemplateModule>
    {
        readonly IUrlGenerator urlGenerator;

        public RemoteHtmlTemplateModuleRenderer(IUrlGenerator urlGenerator)
        {
            this.urlGenerator = urlGenerator;
        }

        public IHtmlString Render(HtmlTemplateModule module)
        {
            return new HtmlString(
                string.Format(
                    "<script src=\"{0}\" type=\"text/javascript\"></script>",
                    urlGenerator.CreateModuleUrl(module)
                )
            );
        }
    }
}
