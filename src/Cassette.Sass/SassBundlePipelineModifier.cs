using Cassette.BundleProcessing;

namespace Cassette.Stylesheets
{
    public class SassBundlePipelineModifier : IBundlePipelineModifier<StylesheetBundle>
    {
        public IBundlePipeline<StylesheetBundle> Modify(IBundlePipeline<StylesheetBundle> pipeline)
        {
            return pipeline.CompileSass();
        }
    }
}