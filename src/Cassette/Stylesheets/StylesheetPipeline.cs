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
        }

        public IAssetTransformer StylesheetMinifier { get; set; }
       
        // TODO: Delete this property
        public bool ConvertImageUrlsToDataUris { get; set; }

        protected override IEnumerable<IBundleProcessor<StylesheetBundle>> CreatePipeline(StylesheetBundle bundle, CassetteSettings settings)
        {
            yield return new AssignStylesheetRenderer();
            yield return new ParseCssReferences();
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
