using Cassette.Assets.Scripts;
using Cassette.Assets.Stylesheets;
using Cassette.Assets.HtmlTemplates;

namespace Cassette.Web
{
    /// <summary>
    /// Global entry point for all view page helpers.
    /// </summary>
    public static class Assets
    {
        public static ScriptAssetManager Scripts
        {
            get
            {
                return CassetteHttpModule.GetPageHelper().ScriptAssetManager;
            }
        }

        public static StylesheetAssetManager Stylesheets
        {
            get
            {
                return CassetteHttpModule.GetPageHelper().StylesheetAssetManager;
            }
        }

        public static HtmlTemplateAssetManager HtmlTemplates
        {
            get
            {
                return CassetteHttpModule.GetPageHelper().HtmlTemplateAssetManager;
            }
        }
    }
}
