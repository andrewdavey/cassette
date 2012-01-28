using System.Collections.Generic;
using System.Linq;

namespace Cassette
{
    class BundleManifestBuilder<TBundle, TManifest> : IBundleVisitor
        where TBundle : Bundle
        where TManifest : BundleManifest, new()
    {
        TManifest bundleManifest;

        public virtual TManifest BuildManifest(TBundle bundle)
        {
            bundle.Accept(this);
            return bundleManifest;
        }

        void IBundleVisitor.Visit(Bundle bundle)
        {
            bundleManifest = new TManifest
            {
                Path = bundle.Path,
                ContentType = bundle.ContentType,
                PageLocation = bundle.PageLocation,
                Assets = new List<AssetManifest>(),
                References = bundle.References.ToList()
            };
        }

        void IBundleVisitor.Visit(IAsset asset)
        {
            bundleManifest.Assets.Add(new AssetManifest
            {
                Path = asset.SourceFile.FullPath,
                RawFileReferences = GetRawFileReferences(asset)
            });
        }

        List<string> GetRawFileReferences(IAsset asset)
        {
            return (from r in asset.References
                    where r.Type == AssetReferenceType.RawFilename
                    select r.Path).ToList();
        }
    }
}