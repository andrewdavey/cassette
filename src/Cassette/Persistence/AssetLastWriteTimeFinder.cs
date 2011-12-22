using System;
using System.Collections.Generic;

namespace Cassette.Persistence
{
    class AssetLastWriteTimeFinder : IBundleVisitor
    {
        DateTime max;

        public DateTime MaxLastWriteTimeUtc
        {
            get { return max; }
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
            var lastWriteTimeUtc = asset.SourceFile.LastWriteTimeUtc;
            if (lastWriteTimeUtc > MaxLastWriteTimeUtc)
            {
                max = lastWriteTimeUtc;
            }
        }
    }
}
