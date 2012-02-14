using Cassette.BundleProcessing;

namespace Cassette.HtmlTemplates
{
    public class RegisterTemplatesWithHogan : AddTransformerToAssets
    {
        public RegisterTemplatesWithHogan(HtmlTemplateBundle bundle, string javaScriptVariableName)
            : base(new RegisterTemplateWithHogan(bundle, javaScriptVariableName))
        {
        }
    }
}