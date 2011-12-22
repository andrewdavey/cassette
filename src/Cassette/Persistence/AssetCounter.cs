using System.Collections.Generic;

namespace Cassette.Persistence
{
    class AssetCounter : IBundleVisitor
    {
        int count;

        public int Count
        {
            get { return count; }
        }

        public void Visit(IEnumerable<Bundle> unprocessedSourceBundles)
        {
            foreach (var bundle in unprocessedSourceBundles)
            {
                bundle.Accept(this);
            }
        }

        public void Visit(Bundle bundle)
        {
        }

        public void Visit(IAsset asset)
        {
            count++;
        }
    }
}
