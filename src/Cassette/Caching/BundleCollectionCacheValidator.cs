using System;

namespace Cassette.Caching
{
    class BundleCollectionCacheValidator : IBundleVisitor
    {
        readonly Func<Type, IAssetCacheValidator> createAssetCacheValidator;
        DateTime asOfDateTime;
        bool isValid;

        public BundleCollectionCacheValidator(Func<Type, IAssetCacheValidator> createAssetCacheValidator)
        {
            this.createAssetCacheValidator = createAssetCacheValidator;
        }

        public bool IsValid(CacheReadResult cacheReadResult)
        {
            asOfDateTime = cacheReadResult.CacheCreationDate;
            isValid = true;
            foreach (var bundle in cacheReadResult.Bundles)
            {
                bundle.Accept(this);
            }
            return isValid;
        }

        void IBundleVisitor.Visit(Bundle bundle)
        {
        }

        void IBundleVisitor.Visit(IAsset asset)
        {
            if (!isValid) return; // Once invalid detected there's no point checking the others.

            var validator = createAssetCacheValidator(asset.AssetCacheValidatorType);
            if (!validator.IsValid(asset.Path, asOfDateTime))
            {
                isValid = false;
            }
        }
    }
}