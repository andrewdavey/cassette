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
            CompileLess = true;
        }

        public IAssetTransformer StylesheetMinifier { get; set; }
        public bool CompileLess { get; set; }
        public bool ConvertImageUrlsToDataUris { get; set; }
        public bool ConvertFontUrlsToDataUris { get; set; }

        protected override IEnumerable<IBundleProcessor<StylesheetBundle>> CreatePipeline(StylesheetBundle bundle, CassetteSettings settings)
        {
            yield return new AssignStylesheetRenderer();
            if (bundle.IsFromCache) yield break;

            yield return new ParseCssReferences();
            if (CompileLess)
            {
                yield return new ParseLessReferences();
                yield return new CompileLess(new LessCompiler());
            }
            //if (ConvertImageUrlsToDataUris)
            //{
            //    yield return new ConvertImageUrlsToDataUris();
            //}
            if (ConvertFontUrlsToDataUris)
            {
                yield return new ConvertFontUrlsToDataUris();
            }
            yield return new ExpandCssUrls();
            yield return new SortAssetsByDependency();
            if (!settings.IsDebuggingEnabled)
            {
                yield return new ConcatenateAssets();
                yield return new MinifyAssets(StylesheetMinifier);
            }
        }
    }
}
