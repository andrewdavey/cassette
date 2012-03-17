using Cassette.BundleProcessing;
using Cassette.Configuration;

namespace Cassette.HtmlTemplates
{
    public class CompileKnockoutJQueryTmpl : AddTransformerToAssets<HtmlTemplateBundle>
    {
        protected override IAssetTransformer CreateAssetTransformer(HtmlTemplateBundle bundle, CassetteSettings settings)
        {
            return new CompileAsset(new KnockoutJQueryTmplCompiler(), settings.SourceDirectory);
        }
    }
}