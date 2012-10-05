using System;

namespace Cassette.BundleProcessing
{
    public abstract class AddTransformerToAssets<T> : IBundleProcessor<T>
        where T : Bundle
    {
        public virtual void Process(T bundle)
        {
            var assetTransformer = CreateAssetTransformer(bundle);
            foreach (var asset in bundle.Assets)
            {
                asset.AddAssetTransformer(assetTransformer);
            }
        }

        protected abstract IAssetTransformer CreateAssetTransformer(T bundle);
    }
}