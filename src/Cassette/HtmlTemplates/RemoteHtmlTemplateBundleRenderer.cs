namespace Cassette.HtmlTemplates
{
    class RemoteHtmlTemplateBundleRenderer : IBundleHtmlRenderer<HtmlTemplateBundle>
    {
        readonly IUrlGenerator urlGenerator;

        public RemoteHtmlTemplateBundleRenderer(IUrlGenerator urlGenerator)
        {
            this.urlGenerator = urlGenerator;
        }

        public string Render(HtmlTemplateBundle bundle)
        { 
            return string.Format(
                HtmlConstants.RemoteHtml,
                urlGenerator.CreateBundleUrl(bundle),
                bundle.HtmlAttributes.CombinedAttributes
            );
        }
    }
}