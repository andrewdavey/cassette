using Cassette.BundleProcessing;
using Cassette.Configuration;

namespace Cassette.Stylesheets
{
    public class ExpandCssUrls : IBundleProcessor<StylesheetBundle>
    {
        public void Process(StylesheetBundle bundle, CassetteSettings settings)
        {
            foreach (var asset in bundle.Assets)
            {
                asset.AddAssetTransformer(new ExpandCssUrlsAssetTransformer(settings.SourceDirectory, settings.UrlGenerator));
            }
        }
    }
}