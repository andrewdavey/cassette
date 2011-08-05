using System;
using System.Linq;
using Cassette.CoffeeScript;

namespace Cassette
{
    public class CompileCoffeeScript : IModuleProcessor<Module>
    {
        public CompileCoffeeScript(ICoffeeScriptCompiler coffeeScriptCompiler)
        {
            this.coffeeScriptCompiler = coffeeScriptCompiler;
        }

        readonly ICoffeeScriptCompiler coffeeScriptCompiler;

        public void Process(Module module)
        {
            var coffeeScriptAssets = module.Assets.Where(IsCoffeeScriptAsset);
            foreach (var asset in coffeeScriptAssets)
            {
                asset.AddAssetTransformer(new CompileCoffeeScriptAsset(coffeeScriptCompiler));
            }
        }

        bool IsCoffeeScriptAsset(IAsset asset)
        {
            return asset.SourceFilename.EndsWith(".coffee", StringComparison.OrdinalIgnoreCase);
        }
    }
}
