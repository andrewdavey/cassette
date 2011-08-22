using System;
using Cassette.HtmlTemplates;
using Cassette.Scripts;
using Cassette.Stylesheets;

namespace Cassette.UI
{
    public static class Assets
    {
        public static Func<ICassetteApplication> GetApplication;

        public static IPageAssetManager Scripts
        {
            get { return GetApplication().GetPageAssetManager<ScriptModule>(); }
        }

        public static IPageAssetManager Stylesheets
        {
            get { return GetApplication().GetPageAssetManager<StylesheetModule>(); }
        }

        public static IPageAssetManager HtmlTemplates
        {
            get { return GetApplication().GetPageAssetManager<HtmlTemplateModule>(); }
        }
    }
}
