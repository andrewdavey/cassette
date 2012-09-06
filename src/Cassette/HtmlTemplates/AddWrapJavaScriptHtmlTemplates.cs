using Cassette.BundleProcessing;

namespace Cassette.HtmlTemplates
{
    public class AddWrapJavaScriptHtmlTemplates : IBundleProcessor<HtmlTemplateBundle>
    {
        public void Process(HtmlTemplateBundle bundle)
        {
            bundle.Assets[0].AddAssetTransformer(new WrapJavaScriptHtmlTemplates(bundle.ContentType));
        }
    }
}