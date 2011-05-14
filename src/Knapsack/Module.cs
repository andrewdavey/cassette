using System;
using System.Linq;
using System.Security.Cryptography;

namespace Knapsack
{
    public class Module
    {
        readonly string path;
        readonly Resource[] resources;
        readonly string[] moduleReferences;
        readonly byte[] hash;

        public Module(string path, Resource[] resources, string[] moduleReferences)
        {
            if (!resources.All(s => s.Path.StartsWith(path, StringComparison.OrdinalIgnoreCase)))
                throw new ArgumentException("Resource paths in this module must start with the path \"" + path + "\".");

            this.path = path;
            this.resources = resources;
            this.hash = HashScriptHashes(resources);
            this.moduleReferences = moduleReferences.Select(r => r).ToArray();
        }

        public string Path
        {
            get { return path; }
        }

        public Resource[] Resources
        {
            get { return resources; }
        }

        public string[] References
        {
            get { return moduleReferences; }
        }

        public byte[] Hash
        {
            get { return hash; }
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

        byte[] HashScriptHashes(Resource[] scripts)
        {
            using (var sha1 = SHA1.Create())
            {
                return sha1.ComputeHash(scripts.SelectMany(script => script.Hash).ToArray());
            }
        }

        bool HashEqual(byte[] otherHash)
        {
            return otherHash.Zip(hash, (x, y) => x == y).All(equal => equal);
        }
    }
}
