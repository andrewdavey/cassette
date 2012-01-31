using System.Collections.Generic;

namespace Cassette.Manifests
{
    class BundleManifestBuilder<TBundle, TManifest> : IBundleVisitor
        where TBundle : Bundle
        where TManifest : BundleManifest, new()
    {
        TManifest bundleManifest;

        public bool IncludeContent { get; set; }

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
            if (!IncludeContent || bundle.Assets.Count == 0) return null;
            using (var stream = bundle.OpenStream())
            {
                var bytes = new byte[stream.Length];
                stream.Read(bytes, 0, bytes.Length);
                return bytes;
            }
        }

        void IBundleVisitor.Visit(IAsset asset)
        {
            var assetManifest = new AssetManifestBuilder().BuildManifest(asset);
            bundleManifest.Assets.Add(assetManifest);
        }
    }
}