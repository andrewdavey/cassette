using Cassette.BundleProcessing;
using Cassette.Scripts;

namespace Cassette.RequireJS
{
    public class ScriptPipelineModifier : IBundlePipelineModifier<ScriptBundle>
    {
        public IBundlePipeline<ScriptBundle> Modify(IBundlePipeline<ScriptBundle> pipeline)
        {
            pipeline.Insert<AddDefineCallTransformerToAssets>(0);
            return pipeline;
        }
    }
}