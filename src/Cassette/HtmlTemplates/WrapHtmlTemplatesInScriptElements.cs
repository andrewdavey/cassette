using Cassette.BundleProcessing;
using Cassette.Configuration;

namespace Cassette.HtmlTemplates
{
    public class WrapHtmlTemplatesInScriptElements : IBundleProcessor<HtmlTemplateBundle>
    {
        public void Process(HtmlTemplateBundle bundle, CassetteSettings settings)
        {
            foreach (var asset in bundle.Assets)
            {
                asset.AddAssetTransformer(new WrapHtmlTemplateInScriptElement(bundle));
            }
        }
    }
}
