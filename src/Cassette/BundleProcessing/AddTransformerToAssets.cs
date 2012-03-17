using Cassette.Configuration;

namespace Cassette.BundleProcessing
{
    public abstract class AddTransformerToAssets<T> : IBundleProcessor<T>
        where T : Bundle
    {
        public void Process(T bundle, CassetteSettings settings)
        {
            var assetTransformer = CreateAssetTransformer(bundle, settings);
            foreach (var asset in bundle.Assets)
            {
                asset.AddAssetTransformer(assetTransformer);
            }
        }

        protected abstract IAssetTransformer CreateAssetTransformer(T bundle, CassetteSettings settings);
    }
}