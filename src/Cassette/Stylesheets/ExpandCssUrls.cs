using Cassette.BundleProcessing;
using Cassette.Configuration;

namespace Cassette.Stylesheets
{
    public class ExpandCssUrls : IBundleProcessor<StylesheetBundle>
    {
        readonly IUrlGenerator urlGenerator;

        public ExpandCssUrls(IUrlGenerator urlGenerator)
        {
            this.urlGenerator = urlGenerator;
        }

        public void Process(StylesheetBundle bundle, CassetteSettings settings)
        {
            foreach (var asset in bundle.Assets)
            {
                asset.AddAssetTransformer(new ExpandCssUrlsAssetTransformer(settings.SourceDirectory, urlGenerator));
            }
        }
    }
}