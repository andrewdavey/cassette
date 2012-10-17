using Cassette.BundleProcessing;
using Cassette.Scripts;
using Cassette.TinyIoC;

namespace Cassette.RequireJS
{
    public class X : IBundlePipelineModifier<ScriptBundle>
    {
        public IBundlePipeline<ScriptBundle> Modify(IBundlePipeline<ScriptBundle> pipeline)
        {
            pipeline.Insert<AddDefineCallTransformerToAssets>(0);
            return pipeline;
        }
    }
}