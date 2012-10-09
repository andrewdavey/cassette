using Cassette.BundleProcessing;

namespace Cassette.HtmlTemplates
{
    public class WrapHtmlTemplatesInScriptElements : IBundleProcessor<HtmlTemplateBundle>
    {
        readonly IHtmlTemplateIdStrategy idStrategy;

        public WrapHtmlTemplatesInScriptElements(IHtmlTemplateIdStrategy idStrategy)
        {
            this.idStrategy = idStrategy;
        }

        public void Process(HtmlTemplateBundle bundle)
        {
            foreach (var asset in bundle.Assets)
            {
                asset.AddAssetTransformer(new WrapHtmlTemplateInScriptElement(bundle, idStrategy));
            }
        }
    }
}
