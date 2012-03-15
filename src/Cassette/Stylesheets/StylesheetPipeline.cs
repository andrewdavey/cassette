using System.Collections.Generic;
using Cassette.BundleProcessing;
using Cassette.Configuration;

namespace Cassette.Stylesheets
{
    public class StylesheetPipeline : MutablePipeline<StylesheetBundle>
    {
        public StylesheetPipeline()
        {
            StylesheetMinifier = new MicrosoftStylesheetMinifier();
            CompileSass = true;
        }

        public IAssetTransformer StylesheetMinifier { get; set; }
        public bool CompileSass { get; set; }
        // TODO: Obselete this property in next version
        // Use the EmbedImages extension method instead.
        public bool ConvertImageUrlsToDataUris { get; set; }

        protected override IEnumerable<IBundleProcessor<StylesheetBundle>> CreatePipeline(StylesheetBundle bundle, CassetteSettings settings)
        {
            yield return new AssignStylesheetRenderer();
            yield return new ParseCssReferences();
#if NET40
            if (CompileSass)
            {
                yield return new ParseSassReferences();
                yield return new CompileSass(new SassCompiler());
            }
#endif
            if (ConvertImageUrlsToDataUris)
            {
                yield return new ConvertImageUrlsToDataUris();
            }
            yield return new ExpandCssUrls();
            yield return new SortAssetsByDependency();
            yield return new AssignHash();
            if (!settings.IsDebuggingEnabled)
            {
                yield return new ConcatenateAssets();
                yield return new MinifyAssets(StylesheetMinifier);
            }
        }
    }
}
