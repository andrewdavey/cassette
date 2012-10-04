using System;
using Cassette.BundleProcessing;

namespace Cassette.HtmlTemplates
{
    public class WrapJavaScriptArrayHtmlTemplates : IBundleProcessor<HtmlTemplateBundle>
    {
        public void Process(HtmlTemplateBundle bundle)
        {
            if (bundle.Assets.Count == 0) return;
            if (bundle.Assets.Count > 1) throw new ArgumentException("WrapJavaScriptArrayHtmlTemplates should only process a bundle where the assets have been concatenated.", "bundle");

            var transformer = new WrapJavaScriptHtmlTemplatesTransformer(bundle.ContentType);
            bundle.Assets[0].AddAssetTransformer(transformer);
        }
    }
}