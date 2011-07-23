using Cassette.Assets.Scripts;
using Cassette.Assets.Stylesheets;
using Cassette.Assets.HtmlTemplates;

namespace Cassette
{
    public interface IPageAssetManager
    {
        ScriptAssetManager ScriptAssetManager { get; }
        StylesheetAssetManager StylesheetAssetManager { get; }
        HtmlTemplateAssetManager HtmlTemplateAssetManager { get; }
    }
}