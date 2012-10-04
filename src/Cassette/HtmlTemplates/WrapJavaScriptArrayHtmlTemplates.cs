using System;
using Cassette.BundleProcessing;

namespace Cassette.HtmlTemplates
{
    public class WrapJavaScriptArrayHtmlTemplates : IBundleProcessor<HtmlTemplateBundle>
    {
        readonly string javaScriptVariableName;

        public WrapJavaScriptArrayHtmlTemplates(string javaScriptVariableName)
        {
            if (string.IsNullOrEmpty(javaScriptVariableName))
            {
                javaScriptVariableName = "JST";
            }

            this.javaScriptVariableName = javaScriptVariableName;
        }

        public void Process(HtmlTemplateBundle bundle)
        {
            if (bundle.Assets.Count == 0) return;
            if (bundle.Assets.Count > 1) throw new ArgumentException("WrapJavaScriptArrayHtmlTemplates should only process a bundle where the assets have been concatenated.", "bundle");

            var transformer = new WrapJavaScriptArrayHtmlTemplatesTransformer(javaScriptVariableName);
            bundle.Assets[0].AddAssetTransformer(transformer);
        }
    }
}