using System.Collections.Generic;
using Cassette.IO;

namespace Cassette
{
    public class BundleDescriptor
    {
        public BundleDescriptor()
        {
            AssetFilenames = new List<string>();
            References = new List<string>();
        }

        public List<string> AssetFilenames { get; private set; }
        public List<string> References { get; private set; }
        public string ExternalUrl { get; set; }
        public string FallbackCondition { get; set; }
        public IFile File { get; set; }

        public bool IsFromFile
        {
            get { return File != null; }
        }
    }
}