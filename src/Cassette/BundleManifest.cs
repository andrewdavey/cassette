using System.Collections.Generic;

namespace Cassette
{
    class BundleManifest
    {
        public string Path { get; set; }
        public string ContentType { get; set; }
        public string PageLocation { get; set; }
        public IList<AssetManifest> Assets { get; set; }
        public IList<string> References { get; set; }
    }
}