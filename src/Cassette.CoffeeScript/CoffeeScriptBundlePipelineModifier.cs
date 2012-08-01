using Cassette.BundleProcessing;

namespace Cassette.Scripts
{
    /// <summary>
    /// Inserts CoffeeScript reference parsing and compilation in script bundle pipelines.
    /// </summary>
    public class CoffeeScriptBundlePipelineModifier : IBundlePipelineModifier<ScriptBundle>
    {
        public IBundlePipeline<ScriptBundle> Modify(IBundlePipeline<ScriptBundle> pipeline)
        {
            var index = pipeline.IndexOf<ParseJavaScriptReferences>();
            pipeline.Insert<ParseCoffeeScriptReferences>(index + 1);
            pipeline.Insert<CompileCoffeeScript>(index + 2);
            return pipeline;
        }
    }
}