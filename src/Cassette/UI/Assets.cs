using Cassette.HtmlTemplates;
using Cassette.Scripts;
using Cassette.Stylesheets;

namespace Cassette.UI
{
    public static class Assets
    {
        public static ICassetteApplication Application;

        public static IPageAssetManager Scripts
        {
            get { return Application.GetPageAssetManager<ScriptModule>(); }
        }

        public static IPageAssetManager Stylesheets
        {
            get { return Application.GetPageAssetManager<StylesheetModule>(); }
        }

        public static IPageAssetManager HtmlTemplates
        {
            get { return Application.GetPageAssetManager<HtmlTemplateModule>(); }
        }
    }
}
