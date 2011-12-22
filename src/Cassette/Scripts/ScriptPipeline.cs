using System.Collections.Generic;
using Cassette.BundleProcessing;
using Cassette.Configuration;

namespace Cassette.Scripts
{
    public class ScriptPipeline : MutablePipeline<ScriptBundle>
    {
        public ScriptPipeline()
        {
            Minifier = new MicrosoftJavaScriptMinifier();
            CompileCoffeeScript = true;
            // Default to the slower, but medium trust compatible, compiler.
            CoffeeScriptCompiler = new JurassicCoffeeScriptCompiler();
        }

        public bool CompileCoffeeScript { get; set; }
        public ICompiler CoffeeScriptCompiler { get; set; }
        public IAssetTransformer Minifier { get; set; }

        protected override IEnumerable<IBundleProcessor<ScriptBundle>> CreatePipeline(ScriptBundle bundle, CassetteSettings settings)
        {
            yield return new AssignScriptRenderer();
            if (bundle.IsFromCache) yield break;

            yield return new ParseJavaScriptReferences();
            if (CompileCoffeeScript)
            {
                yield return new ParseCoffeeScriptReferences();
                yield return new CompileCoffeeScript(CoffeeScriptCompiler);
            }
            yield return new SortAssetsByDependency();
            if (!settings.IsDebuggingEnabled)
            {
                yield return new ConcatenateAssets();
                yield return new MinifyAssets(Minifier);
            }
        }
    }
}