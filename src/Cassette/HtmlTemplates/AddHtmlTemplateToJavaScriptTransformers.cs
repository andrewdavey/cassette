using Cassette.BundleProcessing;

namespace Cassette.HtmlTemplates
{
    public class AddHtmlTemplateToJavaScriptTransformers : IBundleProcessor<HtmlTemplateBundle>
    {
        readonly HtmlTemplateToJavaScriptTransformer.Factory createTransformer;

        public AddHtmlTemplateToJavaScriptTransformers(HtmlTemplateToJavaScriptTransformer.Factory createTransformer)
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