using Cassette.CoffeeScript;

namespace Cassette.ModuleProcessing
{
    public class CompileCoffeeScript : ModuleProcessorOfAssetsMatchingFileExtension<Module>
    {
        public CompileCoffeeScript(ICoffeeScriptCompiler coffeeScriptCompiler)
            : base("coffee")
        {
            this.coffeeScriptCompiler = coffeeScriptCompiler;
        }

        readonly ICoffeeScriptCompiler coffeeScriptCompiler;

        protected override void Process(IAsset asset, Module module)
        {
            asset.AddAssetTransformer(new CompileCoffeeScriptAsset(coffeeScriptCompiler));
        }
    }
}