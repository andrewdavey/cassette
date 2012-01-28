
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
                "<script src=\"{0}\" type=\"text/javascript\"{1}></script>",
                urlGenerator.CreateBundleUrl(bundle),
                bundle.HtmlAttributes.CombinedAttributes
            );
        }
    }
}