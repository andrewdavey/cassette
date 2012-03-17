using Cassette.BundleProcessing;
using Cassette.Configuration;

namespace Cassette.HtmlTemplates
{
    public class RegisterTemplatesWithHogan : AddTransformerToAssets<HtmlTemplateBundle>
    {
        readonly string javaScriptVariableName;

        public RegisterTemplatesWithHogan(string javaScriptVariableName)
        {
            this.javaScriptVariableName = javaScriptVariableName;
        }

        protected override IAssetTransformer CreateAssetTransformer(HtmlTemplateBundle bundle, CassetteSettings settings)
        {
            return new RegisterTemplateWithHogan(bundle, javaScriptVariableName);
        }
    }
}