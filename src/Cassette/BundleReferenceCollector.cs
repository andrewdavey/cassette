using System.Collections.Generic;
using System.Linq;

namespace Cassette
{
    class BundleReferenceCollector : IBundleVisitor
    {
        readonly HashSet<AssetReferenceType> validTypes;

        public BundleReferenceCollector(params AssetReferenceType[] typesToCollect)
        {
            CollectedAssetReferences = new List<AssetReference>();
            validTypes = new HashSet<AssetReferenceType>(typesToCollect);
        }

        public List<AssetReference> CollectedAssetReferences { get; private set; }

        public void Visit(Bundle bundle)
        {
        }

        public void Visit(IAsset asset)
        {
            var assetReferencesToDifferentBundle = asset.References.Where(ShouldCollectReference);
            foreach (var reference in assetReferencesToDifferentBundle)
            {
                CollectedAssetReferences.Add(reference);
            }
        }

        bool ShouldCollectReference(AssetReference reference)
        {
            return validTypes.Contains(reference.Type);
        }
    }
}