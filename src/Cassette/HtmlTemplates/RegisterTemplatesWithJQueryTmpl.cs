using Cassette.BundleProcessing;
using Cassette.Configuration;

namespace Cassette.HtmlTemplates
{
    public class RegisterTemplatesWithJQueryTmpl : AddTransformerToAssets<HtmlTemplateBundle>
    {
        protected override IAssetTransformer CreateAssetTransformer(HtmlTemplateBundle bundle, CassetteSettings settings)
        {
            return new RegisterTemplateWithJQueryTmpl(bundle);
        }
    }
}
