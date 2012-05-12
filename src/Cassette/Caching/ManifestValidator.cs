using System;
using Cassette.IO;

namespace Cassette.Caching
{
    class ManifestValidator : IBundleVisitor
    {
        readonly Func<Type, IAssetCacheValidator> createAssetCacheValidator;
        readonly IDirectory sourceDirectory;
        DateTime asOfDateTime;
        bool isValid;

        public ManifestValidator(Func<Type, IAssetCacheValidator> createAssetCacheValidator, IDirectory sourceDirectory)
        {
            this.createAssetCacheValidator = createAssetCacheValidator;
            this.sourceDirectory = sourceDirectory;
        }

        public bool IsValid(Manifest manifest)
        {
            asOfDateTime = manifest.CreationDateTime;
            isValid = true;
            foreach (var bundle in manifest.Bundles)
            {
                bundle.Accept(this);
                if (!isValid) break; // Once invalid detected there's no point checking the others.
            }
            return isValid;
        }

        void IBundleVisitor.Visit(Bundle bundle)
        {
            if (!bundle.IsFromDescriptorFile) return;

            var descriptorFile = sourceDirectory.GetFile(bundle.DescriptorFilePath);
            if (!descriptorFile.Exists || descriptorFile.LastWriteTimeUtc > asOfDateTime)
            {
                isValid = false;                    
            }
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