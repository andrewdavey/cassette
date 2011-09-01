using System.Collections.Generic;

namespace Cassette
{
    public class ModuleDescriptor
    {
        readonly IEnumerable<string> assetFilenames;
        readonly bool assetsSorted;
        readonly IEnumerable<string> references;
        readonly string externalUrl;
        readonly string fallbackCondition;

        public ModuleDescriptor(IEnumerable<string> assetFilenames, bool assetsSorted, IEnumerable<string> references, string externalUrl = null, string fallbackCondition = null)
        {
            this.assetFilenames = assetFilenames;
            this.assetsSorted = assetsSorted;
            this.references = references;
            this.externalUrl = externalUrl;
            this.fallbackCondition = fallbackCondition;
        }

        public IFile SourceFile { get; set; }

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

        public string ExternalUrl
        {
            get { return externalUrl; }
        }

        public string FallbackCondition
        {
            get { return fallbackCondition; }
        }
    }
}
