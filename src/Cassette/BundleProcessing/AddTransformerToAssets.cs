using Cassette.Configuration;

namespace Cassette.BundleProcessing
{
    public abstract class AddTransformerToAssets : IBundleProcessor<Bundle>
    {
        protected AddTransformerToAssets(IAssetTransformer assetTransformer)
        {
            this.assetTransformer = assetTransformer;
        }

        protected readonly IAssetTransformer assetTransformer;

        public void Process(Bundle bundle, CassetteSettings settings)
        {
            foreach (var asset in bundle.Assets)
            {
                asset.AddAssetTransformer(assetTransformer);
            }
        }
    }
}