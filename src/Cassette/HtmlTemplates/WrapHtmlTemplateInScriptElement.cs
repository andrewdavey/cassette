namespace Cassette.HtmlTemplates
{
    class WrapHtmlTemplateInScriptElement : IAssetTransformer
    {
        readonly HtmlTemplateBundle bundle;
        readonly IHtmlTemplateIdStrategy idStrategy;

        public WrapHtmlTemplateInScriptElement(HtmlTemplateBundle bundle, IHtmlTemplateIdStrategy idStrategy)
        {
            this.bundle = bundle;
            this.idStrategy = idStrategy;
        }

        public string Transform(string assetContent, IAsset asset)
        {
            return string.Format(
                "<script id=\"{0}\" type=\"{1}\"{2}>{3}</script>",
                idStrategy.HtmlTemplateId(bundle, asset),
                bundle.ContentType,
                bundle.HtmlAttributes.CombinedAttributes,
                assetContent
            );
        }
    }
}