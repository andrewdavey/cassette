using System.Web;

namespace Cassette.Scripts
{
    public class ScriptModuleHtmlRenderer : IModuleHtmlRenderer<ScriptModule>
    {
        public ScriptModuleHtmlRenderer(IUrlGenerator urlGenerator)
        {
            this.urlGenerator = urlGenerator;
        }

        readonly IUrlGenerator urlGenerator;

        public IHtmlString Render(ScriptModule module)
        {
            return new HtmlString(
                string.Format(
                    HtmlConstants.ScriptHtml, 
                    urlGenerator.CreateModuleUrl(module)
                )
            );
        }
    }
}