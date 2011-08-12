using Cassette.Compilation;

namespace Cassette.ModuleProcessing
{
    public class CompileCoffeeScript : ModuleProcessorOfAssetsMatchingFileExtension<Module>
    {
        public CompileCoffeeScript(ICompiler coffeeScriptCompiler)
            : base("coffee")
        {
            this.coffeeScriptCompiler = coffeeScriptCompiler;
        }

        readonly ICompiler coffeeScriptCompiler;

        protected override void Process(IAsset asset, Module module)
        {
            asset.AddAssetTransformer(new CompileAsset(coffeeScriptCompiler));
        }
    }
}