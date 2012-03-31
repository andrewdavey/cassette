using Cassette.BundleProcessing;

namespace Cassette.Stylesheets
{
    public class StylesheetPipeline : BundlePipeline<StylesheetBundle>
    {
        public StylesheetPipeline(IStylesheetMinifier stylesheetMinifier, IUrlGenerator urlGenerator)
        {
            AddRange(new IBundleProcessor<StylesheetBundle>[]
            {
                new AssignStylesheetRenderer(urlGenerator),
                new ParseCssReferences(),
                new ExpandCssUrls(urlGenerator),
                new SortAssetsByDependency(),
                new AssignHash(),
                new ConditionalBundlePipeline<StylesheetBundle>(
                    settings => !settings.IsDebuggingEnabled,
                    new ConcatenateAssets(),
                    new MinifyAssets(stylesheetMinifier)
                )
            });
        }
    }
}