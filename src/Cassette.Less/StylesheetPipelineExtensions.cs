using Cassette.BundleProcessing;

namespace Cassette.Stylesheets
{
    public static class StylesheetPipelineExtensions
    {
        public static T CompileLess<T>(this T pipeline) where T : MutablePipeline<StylesheetBundle>
        {
            pipeline.InsertAfter<ParseCssReferences>(
                new ParseLessReferences(),
                new CompileLess(new LessCompiler())
            );
            return pipeline;
        }
    }
}