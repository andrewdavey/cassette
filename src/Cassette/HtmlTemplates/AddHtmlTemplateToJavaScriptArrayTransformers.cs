using Cassette.BundleProcessing;

namespace Cassette.HtmlTemplates
{
    public class AddHtmlTemplateToJavaScriptArrayTransformers : IBundleProcessor<HtmlTemplateBundle>
    {
        readonly HtmlTemplateToJavaScriptTransformer.Factory createTransformer;

        /// <summary>
        /// The name of the JavaScript variable to store compiled templates in.
        /// For example, the default is "JST", so a template will be registered as <code>JST['my-template'] = ...;</code>.
        /// </summary>
        public string JavaScriptVariableName { get; set; }

        public AddHtmlTemplateToJavaScriptArrayTransformers(HtmlTemplateToJavaScriptTransformer.Factory createTransformer)
        {
            this.createTransformer = createTransformer;
        }

        public void Process(HtmlTemplateBundle bundle)
        {
            var javaScriptVariableName = JavaScriptVariableName;
            if(string.IsNullOrEmpty(javaScriptVariableName))
            {
                javaScriptVariableName = "JST";
            }

            var transformer = createTransformer(javaScriptVariableName + "[{0}]={1};", bundle);
            foreach (var asset in bundle.Assets)
            {
                asset.AddAssetTransformer(transformer);
            }
        }
    }
}