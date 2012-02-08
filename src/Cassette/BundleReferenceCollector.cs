using System.Collections.Generic;
using System.Linq;
using Iesi.Collections.Generic;

namespace Cassette
{
    class BundleReferenceCollector : IBundleVisitor
    {
        readonly HashedSet<AssetReferenceType> validTypes;

        public BundleReferenceCollector(params AssetReferenceType[] typesToCollect)
        {
            CollectedAssetReferences = new List<AssetReference>();
            validTypes = new HashedSet<AssetReferenceType>(typesToCollect);
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