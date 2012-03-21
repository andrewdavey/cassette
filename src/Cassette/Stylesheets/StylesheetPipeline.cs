using Cassette.BundleProcessing;

namespace Cassette.Stylesheets
{
    public class StylesheetPipeline : BundlePipeline<StylesheetBundle>
    {
        public StylesheetPipeline()
            : this(new MicrosoftStylesheetMinifier())
        {
        }

        public StylesheetPipeline(IAssetTransformer cssMinifier)
        {
            AddRange(new IBundleProcessor<StylesheetBundle>[]
            {
                new AssignStylesheetRenderer(),
                new ParseCssReferences(),
                new ExpandCssUrls(),
                new SortAssetsByDependency(),
                new AssignHash(),
                new ConditionalBundlePipeline<StylesheetBundle>(
                    settings => !settings.IsDebuggingEnabled,
                    new ConcatenateAssets(),
                    new MinifyAssets(cssMinifier)
                )
            });
        }
    }
}