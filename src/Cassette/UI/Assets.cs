using Cassette.HtmlTemplates;
using Cassette.Scripts;
using Cassette.Stylesheets;

namespace Cassette.UI
{
    public static class Assets
    {
        public static ICassetteApplication Application;

        public static IPageAssetManager<ScriptModule> Scripts
        {
            get { return Application.GetPageAssetManager<ScriptModule>(); }
        }

        public static IPageAssetManager<StylesheetModule> Stylesheets
        {
            get { return Application.GetPageAssetManager<StylesheetModule>(); }
        }

        public static IPageAssetManager<HtmlTemplateModule> HtmlTemplates
        {
            get { return Application.GetPageAssetManager<HtmlTemplateModule>(); }
        }
    }
}
