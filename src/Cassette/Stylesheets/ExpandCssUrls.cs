using Cassette.BundleProcessing;

namespace Cassette.Stylesheets
{
    public class ExpandCssUrls : IBundleProcessor<StylesheetBundle>
    {
        readonly IUrlGenerator urlGenerator;
        readonly CassetteSettings settings;

        public ExpandCssUrls(IUrlGenerator urlGenerator, CassetteSettings settings)
        {
            this.urlGenerator = urlGenerator;
            this.settings = settings;
        }

        public void Process(StylesheetBundle bundle)
        {
            foreach (var asset in bundle.Assets)
            {
                asset.AddAssetTransformer(new ExpandCssUrlsAssetTransformer(settings.SourceDirectory, urlGenerator));
            }
        }
    }
}