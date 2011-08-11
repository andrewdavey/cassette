using Cassette.Less;

namespace Cassette.ModuleProcessing
{
    public class CompileLess : ModuleProcessorOfAssetsMatchingFileExtension<StylesheetModule>
    {
        public CompileLess(ILessCompiler compiler)
            : base("less")
        {
            this.compiler = compiler;
        }

        readonly ILessCompiler compiler;

        protected override void Process(IAsset asset, Module module)
        {
            asset.AddAssetTransformer(new CompileLessAsset(compiler, module));
        }
    }
}