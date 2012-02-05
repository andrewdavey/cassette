using Cassette.BundleProcessing;

namespace Cassette.HtmlTemplates
{
    public class RegisterTemplatesWithHoganTmpl : AddTransformerToAssets
    {
        public RegisterTemplatesWithHoganTmpl(HtmlTemplateBundle bundle)
            : base(new RegisterTemplateWithHoganTmpl(bundle))
        {
        }
    }
}
