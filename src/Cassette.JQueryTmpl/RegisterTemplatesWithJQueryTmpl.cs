using Cassette.BundleProcessing;

namespace Cassette.HtmlTemplates
{
    public class RegisterTemplatesWithJQueryTmpl : AddTransformerToAssets<HtmlTemplateBundle>
    {
        readonly RegisterTemplateWithJQueryTmpl.Factory createAssetTransformer;

        public RegisterTemplatesWithJQueryTmpl(RegisterTemplateWithJQueryTmpl.Factory createAssetTransformer)
        {
            this.createAssetTransformer = createAssetTransformer;
        }

        protected override IAssetTransformer CreateAssetTransformer(HtmlTemplateBundle bundle)
        {
            return createAssetTransformer(bundle);
        }
    }
}
