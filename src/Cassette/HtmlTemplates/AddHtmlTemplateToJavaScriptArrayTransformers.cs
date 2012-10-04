using Cassette.BundleProcessing;

namespace Cassette.HtmlTemplates
{
    public class AddHtmlTemplateToJavaScriptArrayTransformers : IBundleProcessor<HtmlTemplateBundle>
    {
        readonly HtmlTemplateToJavaScriptTransformer.Factory createTransformer;
        readonly string variableName;

        public AddHtmlTemplateToJavaScriptArrayTransformers(HtmlTemplateToJavaScriptTransformer.Factory createTransformer, string variableName)
        {
            if (string.IsNullOrEmpty(variableName))
                variableName = "JST";

            this.createTransformer = createTransformer;
            this.variableName = variableName;
        }

        public void Process(HtmlTemplateBundle bundle)
        {
            var transformer = createTransformer(this.variableName + "[{0}]={1};", bundle);
            foreach (var asset in bundle.Assets)
            {
                asset.AddAssetTransformer(transformer);
            }
        }
    }
}