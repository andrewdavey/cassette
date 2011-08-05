using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
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
                asset.AddAssetTransformer(new CompileCoffeeScriptAsset());
            }
        }

        bool IsCoffeeScriptAsset(IAsset asset)
        {
            return asset.SourceFilename.EndsWith(".coffee", StringComparison.OrdinalIgnoreCase);
        }
    }
}
