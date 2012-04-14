using Cassette.BundleProcessing;
using Cassette.Configuration;

namespace Cassette.Stylesheets
{
    public class LessBundlePipelineModifier : IBundlePipelineModifier<StylesheetBundle>
    {
        readonly ILessCompiler lessCompiler;
        readonly CassetteSettings settings;

        public LessBundlePipelineModifier(ILessCompiler lessCompiler, CassetteSettings settings)
        {
            this.lessCompiler = lessCompiler;
            this.settings = settings;
        }

        public IBundlePipeline<StylesheetBundle> Modify(IBundlePipeline<StylesheetBundle> pipeline)
        {
            pipeline.InsertAfter<ParseCssReferences, StylesheetBundle>(
                new ParseLessReferences(),
                new CompileLess(lessCompiler, settings)
            );
            return pipeline;
        }
    }
}