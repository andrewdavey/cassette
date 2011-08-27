using System.Collections.Generic;

namespace Cassette
{
    public class ModuleDescriptor
    {
        readonly IEnumerable<string> assetFilenames;
        readonly IEnumerable<string> references;

        public ModuleDescriptor(IEnumerable<string> assetFilenames, IEnumerable<string> references)
        {
            this.assetFilenames = assetFilenames;
            this.references = references;
        }

        public IEnumerable<string> AssetFilenames
        {
            get { return assetFilenames; }
        }

        public IEnumerable<string> References
        {
            get { return references; }
        }
    }
}
