using Cassette.BundleProcessing;

namespace Cassette.HtmlTemplates
{
    public class RegisterTemplatesWithHogan : AddTransformerToAssets
    {
        public RegisterTemplatesWithHogan(HtmlTemplateBundle bundle)
            : base(new RegisterTemplateWithHogan(bundle))
        {
        }
    }
}
