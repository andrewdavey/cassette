using Cassette.Assets.Scripts;
using Cassette.Assets.Stylesheets;
using Cassette.Assets.HtmlTemplates;

namespace Cassette.Web
{
    public interface IPageHelper
    {
        ScriptAssetManager ScriptAssetManager { get; }
        StylesheetAssetManager StylesheetAssetManager { get; }
        HtmlTemplateAssetManager HtmlTemplateAssetManager { get; }
    }
}