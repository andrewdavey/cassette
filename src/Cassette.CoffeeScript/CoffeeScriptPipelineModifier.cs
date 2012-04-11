using Cassette.BundleProcessing;

namespace Cassette.Scripts
{
    public class CoffeeScriptPipelineModifier : IBundlePipelineModifier<ScriptBundle>
    {
        readonly ICoffeeScriptCompiler coffeeScriptCompiler;

        public CoffeeScriptPipelineModifier(ICoffeeScriptCompiler coffeeScriptCompiler)
        {
            this.coffeeScriptCompiler = coffeeScriptCompiler;
        }

        public IBundlePipeline<ScriptBundle> Modify(IBundlePipeline<ScriptBundle> pipeline)
        {
            return pipeline.CompileCoffeeScript(coffeeScriptCompiler);
        }
    }
}