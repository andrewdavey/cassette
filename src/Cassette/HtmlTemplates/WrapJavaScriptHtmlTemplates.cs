using System;
using Cassette.BundleProcessing;

namespace Cassette.HtmlTemplates
{
    public class WrapJavaScriptHtmlTemplates : IBundleProcessor<HtmlTemplateBundle>
    {
        readonly WrapJavaScriptHtmlTemplatesTransformer transformer;

        public WrapJavaScriptHtmlTemplates(WrapJavaScriptHtmlTemplatesTransformer transformer)
        {
            this.transformer = transformer;
        }

        public void Process(HtmlTemplateBundle bundle)
        {
            if (bundle.Assets.Count == 0) return;
            if (bundle.Assets.Count > 1) throw new ArgumentException("WrapJavaScriptHtmlTemplates should only process a bundle where the assets have been concatenated.", "bundle");

            bundle.Assets[0].AddAssetTransformer(transformer);
        }
    }
}