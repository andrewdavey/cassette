using Cassette.BundleProcessing;
using Cassette.Configuration;

namespace Cassette.HtmlTemplates
{
    public class CompileKnockoutJQueryTmpl : AddTransformerToAssets<HtmlTemplateBundle>
    {
        readonly CassetteSettings settings;

        public CompileKnockoutJQueryTmpl(CassetteSettings settings)
        {
            this.settings = settings;
        }

        protected override IAssetTransformer CreateAssetTransformer(HtmlTemplateBundle bundle)
        {
            return new CompileAsset(new KnockoutJQueryTmplCompiler(), settings.SourceDirectory);
        }
    }
}