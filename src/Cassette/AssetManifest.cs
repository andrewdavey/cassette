using System.Collections.Generic;

namespace Cassette
{
    class AssetManifest
    {
        public string Path { get; set; }
        public List<string> RawFileReferences { get; set; }

        public override bool Equals(object obj)
        {
            var other = obj as AssetManifest;
            return other != null && Path.Equals(other.Path);
        }

        public override int GetHashCode()
        {
            return Path.GetHashCode();
        }
    }
}