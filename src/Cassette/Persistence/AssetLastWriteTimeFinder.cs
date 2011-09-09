using System;
using System.Collections.Generic;

namespace Cassette.Persistence
{
    class AssetLastWriteTimeFinder : IAssetVisitor
    {
        DateTime max;

        public DateTime MaxLastWriteTimeUtc
        {
            get { return max; }
        }

        public void Visit(IEnumerable<Module> unprocessedSourceModules)
        {
            foreach (var module in unprocessedSourceModules)
            {
                module.Accept(this);
            }
        }

        public void Visit(Module module)
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