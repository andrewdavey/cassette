namespace Cassette.HtmlTemplates
{
    public interface IHtmlTemplateIdStrategy
    {
        string GetHtmlTemplateId(IAsset htmlTemplateAsset, HtmlTemplateBundle bundle);
    }
}