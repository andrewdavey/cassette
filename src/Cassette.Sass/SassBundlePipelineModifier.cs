using Cassette.BundleProcessing;

namespace Cassette.Stylesheets
{
    public class SassBundlePipelineModifier : IBundlePipelineModifier<StylesheetBundle>
    {
        readonly ISassCompiler sassCompiler;

        public SassBundlePipelineModifier(ISassCompiler sassCompiler)
        {
            this.sassCompiler = sassCompiler;
        }

        public IBundlePipeline<StylesheetBundle> Modify(IBundlePipeline<StylesheetBundle> pipeline)
        {
            pipeline.InsertAfter<ParseCssReferences, StylesheetBundle>(
                new ParseSassReferences(),
                new CompileSass(sassCompiler)
            );
            return pipeline;
        }
    }
}