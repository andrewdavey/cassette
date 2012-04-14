using Cassette.BundleProcessing;

namespace Cassette.HtmlTemplates
{
    public class WrapHtmlTemplatesInScriptElements : IBundleProcessor<HtmlTemplateBundle>
    {
        public void Process(HtmlTemplateBundle bundle)
        {
            foreach (var asset in bundle.Assets)
            {
                asset.AddAssetTransformer(new WrapHtmlTemplateInScriptElement(bundle));
            }
        }
    }
}
