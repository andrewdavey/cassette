using System;

namespace Cassette.Caching
{
    class ManifestValidator : IBundleVisitor
    {
        readonly Func<Type, IAssetCacheValidator> createAssetCacheValidator;
        DateTime asOfDateTime;
        bool isValid;

        public ManifestValidator(Func<Type, IAssetCacheValidator> createAssetCacheValidator)
        {
            this.createAssetCacheValidator = createAssetCacheValidator;
        }

        public bool IsValid(Manifest manifest)
        {
            // TODO: Are precompiled manifests always true?

            asOfDateTime = manifest.CreationDateTime;
            isValid = true;
            foreach (var bundle in manifest.Bundles)
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