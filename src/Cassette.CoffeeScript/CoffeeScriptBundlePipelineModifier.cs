using Cassette.BundleProcessing;

namespace Cassette.Scripts
{
    /// <summary>
    /// Inserts CoffeeScript reference parsing and compilation in script bundle pipelines.
    /// </summary>
    public class CoffeeScriptBundlePipelineModifier : IBundlePipelineModifier<ScriptBundle>
    {
        readonly ICoffeeScriptCompiler coffeeScriptCompiler;

        public CoffeeScriptBundlePipelineModifier(ICoffeeScriptCompiler coffeeScriptCompiler)
        {
            this.coffeeScriptCompiler = coffeeScriptCompiler;
        }

        public IBundlePipeline<ScriptBundle> Modify(IBundlePipeline<ScriptBundle> pipeline)
        {
            pipeline.InsertAfter<ParseJavaScriptReferences, ScriptBundle>(
                new ParseCoffeeScriptReferences(),
                new CompileCoffeeScript(coffeeScriptCompiler)
            );
            return pipeline;
        }
    }
}