using System.Collections.Generic;
using Cassette.Compilation;
using Cassette.ModuleProcessing;

namespace Cassette
{
    public class DefaultScriptPipeline : IModuleProcessor<ScriptModule>
    {
        public DefaultScriptPipeline()
        {
            Minifier = new MicrosoftJavaScriptMinifier();
        }

        public bool CompileCoffeeScript { get; set; }
        public IAssetTransformer Minifier { get; set; }

        public void Process(ScriptModule module, ICassetteApplication application)
        {
            foreach (var processor in CreatePipeline(application))
            {
                processor.Process(module, application);
            }
        }

        IEnumerable<IModuleProcessor<ScriptModule>> CreatePipeline(ICassetteApplication application)
        {
            yield return new ParseJavaScriptReferences();
            if (CompileCoffeeScript)
            {
                yield return new ParseCoffeeScriptReferences();
                yield return new CompileCoffeeScript(application.GetCompiler("coffee"));
            }
            yield return new SortAssetsByDependency();
            if (application.IsOutputOptimized)
            {
                yield return new ConcatenateAssets();
                yield return new MinifyAssets(new MicrosoftJavaScriptMinifier());
            }
        }
    }
}
