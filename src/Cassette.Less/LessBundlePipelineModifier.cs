using Cassette.BundleProcessing;

namespace Cassette.Stylesheets
{
    public class LessBundlePipelineModifier : IBundlePipelineModifier<StylesheetBundle>
    {
        readonly ILessCompiler lessCompiler;

        public LessBundlePipelineModifier(ILessCompiler lessCompiler)
        {
            this.lessCompiler = lessCompiler;
        }

        public IBundlePipeline<StylesheetBundle> Modify(IBundlePipeline<StylesheetBundle> pipeline)
        {
            pipeline.InsertAfter<ParseCssReferences, StylesheetBundle>(
                new ParseLessReferences(),
                new CompileLess(lessCompiler)
            );
            return pipeline;
        }
    }
}