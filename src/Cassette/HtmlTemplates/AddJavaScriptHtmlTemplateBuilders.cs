using Cassette.BundleProcessing;

namespace Cassette.HtmlTemplates
{
    public class AddJavaScriptHtmlTemplateBuilders : IBundleProcessor<HtmlTemplateBundle>
    {
        readonly JavaScriptHtmlTemplateBuilder.Factory createTransformer;

        public AddJavaScriptHtmlTemplateBuilders(JavaScriptHtmlTemplateBuilder.Factory createTransformer)
        {
            this.createTransformer = createTransformer;
        }

        public void Process(HtmlTemplateBundle bundle)
        {
            foreach (var asset in bundle.Assets)
            {
                var transformer = createTransformer(bundle);
                asset.AddAssetTransformer(transformer);
            }
        }
    }
}