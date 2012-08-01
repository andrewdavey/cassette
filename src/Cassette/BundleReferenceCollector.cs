using System.Collections.Generic;
using System.Linq;
using Cassette.Utilities;
#if NET35
using Iesi.Collections.Generic;
#endif

namespace Cassette
{
    class BundleReferenceCollector : IBundleVisitor
    {
        readonly HashedSet<AssetReferenceType> validTypes;
		Bundle currentBundle;

        public BundleReferenceCollector(params AssetReferenceType[] typesToCollect)
        {
            CollectedReferences = new List<CollectedReference>();
            validTypes = new HashedSet<AssetReferenceType>(typesToCollect);
        }

        public List<CollectedReference> CollectedReferences { get; private set; }

        public void Visit(Bundle bundle)
        {
            currentBundle = bundle;
        }

        public void Visit(IAsset asset)
        {
            var assetReferencesToCollect = asset.References.Where(ShouldCollectReference);
            foreach (var reference in assetReferencesToCollect)
            {
                CollectReference(reference);
            }
        }

        void CollectReference(AssetReference reference)
        {
            CollectedReferences.Add(new CollectedReference
            {
                SourceBundle = currentBundle,
                AssetReference = reference
            });
        }

        bool ShouldCollectReference(AssetReference reference)
        {
            return validTypes.Contains(reference.Type);
        }

        public class CollectedReference
        {
            public Bundle SourceBundle;
            public AssetReference AssetReference;
        }
    }
}