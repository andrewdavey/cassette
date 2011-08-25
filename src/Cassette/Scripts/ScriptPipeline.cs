using System.Collections.Generic;
using Cassette.ModuleProcessing;

namespace Cassette.Scripts
{
    public class ScriptPipeline : MutablePipeline<ScriptModule>
    {
        public ScriptPipeline()
        {
            Minifier = new MicrosoftJavaScriptMinifier();
            CompileCoffeeScript = true;
        }

        public bool CompileCoffeeScript { get; set; }
        public IAssetTransformer Minifier { get; set; }

        protected override IEnumerable<IModuleProcessor<ScriptModule>> CreatePipeline(ScriptModule module, ICassetteApplication application)
        {
            yield return new ParseJavaScriptReferences();
            if (CompileCoffeeScript)
            {
                yield return new ParseCoffeeScriptReferences();
                yield return new CompileCoffeeScript(new CoffeeScriptCompiler());
            }
            yield return new SortAssetsByDependency();
            if (application.IsOutputOptimized)
            {
                yield return new ConcatenateAssets();
                yield return new MinifyAssets(Minifier);
            }
        }
    }
}