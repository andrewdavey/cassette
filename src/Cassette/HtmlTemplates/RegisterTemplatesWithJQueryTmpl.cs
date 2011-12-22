using Cassette.BundleProcessing;

namespace Cassette.HtmlTemplates
{
    public class RegisterTemplatesWithJQueryTmpl : AddTransformerToAssets
    {
        public RegisterTemplatesWithJQueryTmpl(HtmlTemplateBundle bundle)
            : base(new RegisterTemplateWithJQueryTmpl(bundle))
        {
        }
    }
}
