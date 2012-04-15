using Cassette.BundleProcessing;

namespace Cassette.Stylesheets
{
    public class LessBundlePipelineModifier : IBundlePipelineModifier<StylesheetBundle>
    {
        public IBundlePipeline<StylesheetBundle> Modify(IBundlePipeline<StylesheetBundle> pipeline)
        {
            var index = pipeline.IndexOf<ParseCssReferences>();
            pipeline.Insert<ParseLessReferences>(index + 1);
            pipeline.Insert<CompileLess>(index+2);

            return pipeline;
        }
    }
}