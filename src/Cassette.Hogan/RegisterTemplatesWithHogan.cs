using Cassette.BundleProcessing;

namespace Cassette.HtmlTemplates
{
    public class RegisterTemplatesWithHogan : AddTransformerToAssets<HtmlTemplateBundle>
    {
        public delegate RegisterTemplatesWithHogan Factory(string javaScriptVariableName);

        readonly string javaScriptVariableName;

        public RegisterTemplatesWithHogan(string javaScriptVariableName)
        {
            this.javaScriptVariableName = javaScriptVariableName;
        }

        protected override IAssetTransformer CreateAssetTransformer(HtmlTemplateBundle bundle)
        {
            return new RegisterTemplateWithHogan(bundle, javaScriptVariableName);
        }
    }
}