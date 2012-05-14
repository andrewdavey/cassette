using Cassette.BundleProcessing;

namespace Cassette.HtmlTemplates
{
    public class CompileHogan : AddTransformerToAssets<HtmlTemplateBundle>
    {
        readonly CassetteSettings settings;

        public CompileHogan(CassetteSettings settings)
        {
            this.settings = settings;
        }

        protected override IAssetTransformer CreateAssetTransformer(HtmlTemplateBundle bundle)
        {
            return new CompileAsset(new HoganCompiler(), settings.SourceDirectory);
        }
    }
}