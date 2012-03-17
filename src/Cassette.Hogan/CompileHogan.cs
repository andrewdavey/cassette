using Cassette.BundleProcessing;

namespace Cassette.HtmlTemplates
{
    public class CompileHogan : AddTransformerToAssets<HtmlTemplateBundle>
    {
        protected override IAssetTransformer CreateAssetTransformer(HtmlTemplateBundle bundle, Configuration.CassetteSettings settings)
        {
            return new CompileAsset(new HoganCompiler(), settings.SourceDirectory);
        }
    }
}