using Cassette.BundleProcessing;

namespace Cassette.HtmlTemplates
{
    public class RegisterTemplatesWithHogan : AddTransformerToAssets
    {
        public RegisterTemplatesWithHogan(HtmlTemplateBundle bundle, string ns = null)
            : base(new RegisterTemplateWithHogan(bundle, ns))
        {
        }
    }
}
