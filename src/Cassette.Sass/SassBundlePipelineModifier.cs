using Cassette.BundleProcessing;

namespace Cassette.Stylesheets
{
    public class SassBundlePipelineModifier : IBundlePipelineModifier<StylesheetBundle>
    {
        public IBundlePipeline<StylesheetBundle> Modify(IBundlePipeline<StylesheetBundle> pipeline)
        {
            var index = pipeline.IndexOf<ParseCssReferences>();
            pipeline.Insert<ParseSassReferences>(index + 1);
            pipeline.Insert<CompileSass>(index + 2);
            return pipeline;
        }
    }
}