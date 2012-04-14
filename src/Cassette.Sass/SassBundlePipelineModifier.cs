using Cassette.BundleProcessing;
using Cassette.Configuration;

namespace Cassette.Stylesheets
{
    public class SassBundlePipelineModifier : IBundlePipelineModifier<StylesheetBundle>
    {
        readonly ISassCompiler sassCompiler;
        readonly CassetteSettings settings;

        public SassBundlePipelineModifier(ISassCompiler sassCompiler, CassetteSettings settings)
        {
            this.sassCompiler = sassCompiler;
            this.settings = settings;
        }

        public IBundlePipeline<StylesheetBundle> Modify(IBundlePipeline<StylesheetBundle> pipeline)
        {
            pipeline.InsertAfter<ParseCssReferences, StylesheetBundle>(
                new ParseSassReferences(),
                new CompileSass(sassCompiler, settings)
            );
            return pipeline;
        }
    }
}