using Cassette.BundleProcessing;
using Cassette.Configuration;

namespace Cassette.HtmlTemplates
{
    public class RegisterTemplatesWithJQueryTmpl : AddTransformerToAssets<HtmlTemplateBundle>
    {
        readonly CassetteSettings settings;

        public RegisterTemplatesWithJQueryTmpl(CassetteSettings settings)
        {
            this.settings = settings;
        }

        protected override IAssetTransformer CreateAssetTransformer(HtmlTemplateBundle bundle)
        {
            return new RegisterTemplateWithJQueryTmpl(bundle);
        }
    }
}
