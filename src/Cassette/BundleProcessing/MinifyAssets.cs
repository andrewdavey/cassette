using Cassette.Configuration;

namespace Cassette.BundleProcessing
{
    public class MinifyAssets : IBundleProcessor<Bundle>
    {
        public MinifyAssets(IAssetTransformer minifier)
        {
            this.minifier = minifier;
        }

        readonly IAssetTransformer minifier;

        public void Process(Bundle bundle, CassetteSettings settings)
        {
            foreach (var asset in bundle.Assets)
            {
                asset.AddAssetTransformer(minifier);
            }
        }
    }
}