namespace Cassette.HtmlTemplates
{
    public interface IHtmlTemplateIdStrategy
    {
        string HtmlTemplateId(HtmlTemplateBundle bundle, IAsset htmlTemplateAsset);
    }
}