using System;
using Cassette.BundleProcessing;
using Cassette.Configuration;

namespace Cassette.Scripts
{
    public class CompileCoffeeScript : IBundleProcessor<Bundle>
    {
        readonly ICompiler coffeeScriptCompiler;

        public CompileCoffeeScript(ICompiler coffeeScriptCompiler)
        {
            this.coffeeScriptCompiler = coffeeScriptCompiler;
        }

        public void Process(Bundle bundle, CassetteSettings settings)
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