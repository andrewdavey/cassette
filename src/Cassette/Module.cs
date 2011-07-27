using System;
using System.Linq;
using System.Security.Cryptography;

namespace Cassette
{
    public class Module
    {
        readonly string path;
        readonly Asset[] assets;
        readonly string[] moduleReferences;
        readonly string location;
        readonly byte[] hash;

        public Module(string path, Asset[] assets, string[] moduleReferences, string location)
        {
            if (!assets.All(s => s.Path.StartsWith(path, StringComparison.OrdinalIgnoreCase)))
                throw new ArgumentException("Asset paths in this module must start with the path \"" + path + "\".");

            this.path = path;
            this.assets = assets;
            this.location = location ?? "";
            this.hash = HashAssetHashes(assets);
            this.moduleReferences = moduleReferences.Select(r => r).ToArray();
        }

        public string Path
        {
            get { return path; }
        }

        public Asset[] Assets
        {
            get { return assets; }
        }

        public string[] References
        {
            get { return moduleReferences; }
        }

        public string Location
        {
            get { return location; }
        }

        public byte[] Hash
        {
            get { return hash; }
        }

        public bool IsExternal
        {
            get
            {
                return assets.Length > 0 && path == assets[0].Path;
            }
        }

        public override bool Equals(object obj)
        {
            return Equals(obj as Module);
        }

        public bool Equals(Module other)
        {
            return other != null
                && other.path == path
                && HashEqual(other.hash);
        }

        public override int GetHashCode()
        {
            return path.GetHashCode() ^ hash.GetHashCode();
        }

        internal static Module CreateExternalModule(string urlString, string location)
        {
            // External module has only one asset (the given url) and no references.
            return new Module(
                urlString,
                new[] { new Asset(urlString, new byte[0], new string[0]) },
                new string[0],
                location
            );
        }

        byte[] HashAssetHashes(Asset[] assets)
        {
            using (var sha1 = SHA1.Create())
            {
                return sha1.ComputeHash(assets.SelectMany(asset => asset.Hash).ToArray());
            }
        }

        bool HashEqual(byte[] otherHash)
        {
            return otherHash.Zip(hash, (x, y) => x == y).All(equal => equal);
        }
    }
}
