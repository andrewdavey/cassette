using System.Collections.Generic;
using System.Linq;

namespace Cassette.Manifests
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
                Content = GetContent(bundle)
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

        byte[] GetContent(Bundle bundle)
        {
            if (bundle.Assets.Count == 0) return null;
            using (var stream = bundle.OpenStream())
            {
                var bytes = new byte[stream.Length];
                stream.Read(bytes, 0, bytes.Length);
                return bytes;
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