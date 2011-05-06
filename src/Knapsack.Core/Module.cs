using System;
using System.Linq;
using System.Security.Cryptography;

namespace Knapsack
{
    public class Module
    {
        readonly string path;
        readonly Script[] scripts;
        readonly string[] moduleReferences;
        readonly byte[] hash;

        public Module(string path, Script[] scripts, string[] moduleReferences)
        {
            if (!scripts.All(s => s.Path.StartsWith(path, StringComparison.OrdinalIgnoreCase)))
                throw new ArgumentException("Script paths in this module must start with the path \"" + path + "\".");

            this.path = path.ToLower();
            this.scripts = scripts;
            this.hash = HashScriptHashes(scripts);
            this.moduleReferences = moduleReferences.Select(r => r.ToLower()).ToArray();
        }

        public string Path
        {
            get { return path; }
        }

        public Script[] Scripts
        {
            get { return scripts; }
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

        byte[] HashScriptHashes(Script[] scripts)
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
