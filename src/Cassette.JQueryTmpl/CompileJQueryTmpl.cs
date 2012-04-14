using Cassette.BundleProcessing;
using Cassette.Configuration;

namespace Cassette.HtmlTemplates
{
    public class CompileJQueryTmpl : AddTransformerToAssets<HtmlTemplateBundle>
    {
        readonly CassetteSettings settings;

        public CompileJQueryTmpl(CassetteSettings settings)
        {
            this.settings = settings;
        }

        protected override IAssetTransformer CreateAssetTransformer(HtmlTemplateBundle bundle)
        {
            return new CompileAsset(new JQueryTmplCompiler(), settings.SourceDirectory);
        }
    }
}