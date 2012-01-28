using System.Collections.Generic;

namespace Cassette
{
    class AssetManifest
    {
        public string Path { get; set; }
        public List<string> RawFileReferences { get; set; }
    }
}