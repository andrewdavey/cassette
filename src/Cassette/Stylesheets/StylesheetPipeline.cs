using Cassette.BundleProcessing;
using Cassette.TinyIoC;

namespace Cassette.Stylesheets
{
    public class StylesheetPipeline : BundlePipeline<StylesheetBundle>
    {
        public StylesheetPipeline(TinyIoCContainer container, CassetteSettings settings) : base(container)
        {
            AddRange(new IBundleProcessor<StylesheetBundle>[]
            {
                container.Resolve<AssignStylesheetRenderer>(),
                new ParseCssReferences(), 
                container.Resolve<ExpandCssUrls>(),
                new SortAssetsByDependency(),
                new AssignHash()
            });

            if (!settings.IsDebuggingEnabled)
            {
                Add(container.Resolve<ConcatenateAssets>());
                var minifier = container.Resolve<IStylesheetMinifier>();
                Add(new MinifyAssets(minifier));
            }
        }
    }
}
