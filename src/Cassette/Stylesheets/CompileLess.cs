using Cassette.ModuleProcessing;

namespace Cassette.Stylesheets
{
    public class CompileLess : ModuleProcessorOfAssetsMatchingFileExtension<StylesheetModule>
    {
        public CompileLess(ICompiler compiler)
            : base("less")
        {
            this.compiler = compiler;
        }

        readonly ICompiler compiler;

        protected override void Process(IAsset asset, Module module)
        {
            asset.AddAssetTransformer(new CompileAsset(compiler));
            module.RegisterCompiledAsset(asset);
        }
    }
}