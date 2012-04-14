using Cassette.BundleProcessing;
using Cassette.Configuration;

namespace Cassette.Scripts
{
    /// <summary>
    /// Inserts CoffeeScript reference parsing and compilation in script bundle pipelines.
    /// </summary>
    public class CoffeeScriptBundlePipelineModifier : IBundlePipelineModifier<ScriptBundle>
    {
        readonly ICoffeeScriptCompiler coffeeScriptCompiler;
        readonly CassetteSettings settings;

        public CoffeeScriptBundlePipelineModifier(ICoffeeScriptCompiler coffeeScriptCompiler, CassetteSettings settings)
        {
            this.coffeeScriptCompiler = coffeeScriptCompiler;
            this.settings = settings;
        }

        public IBundlePipeline<ScriptBundle> Modify(IBundlePipeline<ScriptBundle> pipeline)
        {
            pipeline.InsertAfter<ParseJavaScriptReferences, ScriptBundle>(
                new ParseCoffeeScriptReferences(),
                new CompileCoffeeScript(coffeeScriptCompiler, settings)
            );
            return pipeline;
        }
    }
}