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
                PageLocation = bundle.PageLocation
            };
            AddReferencesToBundleManifest(bundle.References);
        }

        void AddReferencesToBundleManifest(IEnumerable<string> references)
        {
            foreach (var reference in references)
            {
                bundleManifest.References.Add(reference);
            }
        }

        void IBundleVisitor.Visit(IAsset asset)
        {
            var assetManifest = new AssetManifest
            {
                Path = asset.SourceFile.FullPath
            };
            foreach (var reference in GetRawFileReferences(asset))
            {
                assetManifest.RawFileReferences.Add(reference);
            }
            bundleManifest.Assets.Add(assetManifest);
        }

        IEnumerable<string> GetRawFileReferences(IAsset asset)
        {
            return from r in asset.References
                   where r.Type == AssetReferenceType.RawFilename
                   select r.Path;
        }
    }
}