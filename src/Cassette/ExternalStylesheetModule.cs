using System.Web;

namespace Cassette
{
    public class ExternalStylesheetModule : StylesheetModule
    {
        public ExternalStylesheetModule(string url)
            : base("", null)
        {
            this.url = url;
        }

        readonly string url;

        public override IHtmlString Render(ICassetteApplication application)
        {
            return base.Render(application);
        }
    }
}
