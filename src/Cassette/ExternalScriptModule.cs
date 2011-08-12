using System.Web;

namespace Cassette
{
    public class ExternalScriptModule : ScriptModule
    {
        public ExternalScriptModule(string url)
            : base("")
        {
            this.url = url;
        }

        readonly string url;

        public override IHtmlString Render(ICassetteApplication application)
        {
            return new HtmlString(string.Format(scriptHtml, url));
        }
    }
}