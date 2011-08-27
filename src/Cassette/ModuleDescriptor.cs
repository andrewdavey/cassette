using System.Collections.Generic;

namespace Cassette
{
    public class ModuleDescriptor
    {
        readonly IEnumerable<string> assetFilenames;
        readonly bool assetsSorted;
        readonly IEnumerable<string> references;

        public ModuleDescriptor(IEnumerable<string> assetFilenames, bool assetsSorted, IEnumerable<string> references)
        {
            this.assetFilenames = assetFilenames;
            this.assetsSorted = assetsSorted;
            this.references = references;
        }

        public IEnumerable<string> AssetFilenames
        {
            get { return assetFilenames; }
        }

        public bool AssetsSorted
        {
            get { return assetsSorted; }
        }

        public IEnumerable<string> References
        {
            get { return references; }
        }
    }
}
