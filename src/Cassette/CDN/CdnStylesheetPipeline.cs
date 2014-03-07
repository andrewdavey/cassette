using Cassette.BundleProcessing;
using Cassette.Stylesheets;
using Cassette.TinyIoC;

namespace Cassette.CDN
{
    public class CdnStylesheetPipeline : BundlePipeline<CdnStylesheetBundle>
    {
        public CdnStylesheetPipeline(TinyIoCContainer container, CassetteSettings settings)
            : base(container)
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
