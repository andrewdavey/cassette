using System;
using Cassette.BundleProcessing;

namespace Cassette.Scripts
{
    public class CompileCoffeeScript : IBundleProcessor<Bundle>
    {
        readonly ICoffeeScriptCompiler coffeeScriptCompiler;
        readonly CassetteSettings settings;

        public CompileCoffeeScript(ICoffeeScriptCompiler coffeeScriptCompiler, CassetteSettings settings)
        {
            this.coffeeScriptCompiler = coffeeScriptCompiler;
            this.settings = settings;
        }

        public void Process(Bundle bundle)
        {
            foreach (var asset in bundle.Assets)
            {
                if (asset.Path.EndsWith(".coffee", StringComparison.OrdinalIgnoreCase))
                {
                    asset.AddAssetTransformer(new CompileAsset(coffeeScriptCompiler, settings.SourceDirectory));                    
                }
            }
        }
    }
}