namespace Cassette.Web
{
    public interface IPageHelper
    {
        ScriptAssetManager ScriptAssetManager { get; }
        StylesheetAssetManager StylesheetAssetManager { get; }
        HtmlTemplateAssetManager HtmlTemplateAssetManager { get; }
    }
}