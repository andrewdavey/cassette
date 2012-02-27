using Cassette.BundleProcessing;
using Cassette.Configuration;

namespace Cassette.Stylesheets
{
    public class ConvertImagesToSprites : IBundleProcessor<StylesheetBundle>
    {
        public void Process(StylesheetBundle bundle, CassetteSettings settings)
        {
            foreach (var asset in bundle.Assets)
            {
                asset.AddAssetTransformer(new SpriteTransformer(settings.SourceDirectory, settings.UrlGenerator));
            }
        }
    }
}