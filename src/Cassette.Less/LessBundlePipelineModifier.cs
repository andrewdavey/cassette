using Cassette.BundleProcessing;

namespace Cassette.Stylesheets
{
    public class LessBundlePipelineModifier : IBundlePipelineModifier<StylesheetBundle>
    {
        public IBundlePipeline<StylesheetBundle> Modify(IBundlePipeline<StylesheetBundle> pipeline)
        {
            return pipeline.CompileLess();
        }
    }
}