using Cassette.BundleProcessing;

namespace Cassette.HtmlTemplates
{
    public class CompileJQueryTmpl : AddTransformerToAssets<HtmlTemplateBundle>
    {
        protected override IAssetTransformer CreateAssetTransformer(HtmlTemplateBundle bundle, Configuration.CassetteSettings settings)
        {
            return new CompileAsset(new JQueryTmplCompiler(), settings.SourceDirectory);
        }
    }
}