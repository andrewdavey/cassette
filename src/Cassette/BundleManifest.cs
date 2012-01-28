using System.Collections.Generic;
using System.Linq;

namespace Cassette
{
    class BundleManifest
    {
        public BundleManifest()
        {
            Assets = new List<AssetManifest>();
            References = new List<string>();
        }

        public string Path { get; set; }
        public string ContentType { get; set; }
        public string PageLocation { get; set; }
        public IList<AssetManifest> Assets { get; private set; }
        public IList<string> References { get; private set; }

        public override bool Equals(object obj)
        {
            var other = obj as BundleManifest;
            return other != null 
                && GetType() == other.GetType()
                && Path.Equals(other.Path)
                && AssetsEqual(other.Assets);
        }

        bool AssetsEqual(IEnumerable<AssetManifest> assets)
        {
            return Assets.SequenceEqual(assets);
        }

        public override int GetHashCode()
        {
            return Path.GetHashCode();
        }
    }
}