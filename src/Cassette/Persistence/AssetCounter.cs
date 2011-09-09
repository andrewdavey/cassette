using System.Collections.Generic;

namespace Cassette.Persistence
{
    class AssetCounter : IAssetVisitor
    {
        int count;

        public int Count
        {
            get { return count; }
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
            count++;
        }
    }
}