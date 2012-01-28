using System.Collections.Generic;

namespace Cassette
{
    class AssetManifest
    {
        public AssetManifest()
        {
            RawFileReferences = new List<string>();
        }

        public string Path { get; set; }
        public IList<string> RawFileReferences { get; private set; }

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