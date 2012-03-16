using Cassette.BundleProcessing;

namespace Cassette.Stylesheets
{
    public static class StylesheetPipelineExtensions
    {
        public static T CompileSass<T>(this T pipeline)
            where T : MutablePipeline<StylesheetBundle>
        {
            pipeline.InsertAfter<ParseCssReferences>(
                new ParseSassReferences(),
                new CompileSass(new SassCompiler())
            );
            return pipeline;
        }
    }
}
