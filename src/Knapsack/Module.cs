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
            this.path = path;
            this.scripts = scripts;
            this.moduleReferences = moduleReferences;

            this.hash = HashScriptHashes(scripts);
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

        byte[] HashScriptHashes(Script[] scripts)
        {
            using (var sha1 = SHA1.Create())
            {
                return sha1.ComputeHash(scripts.SelectMany(script => script.Hash).ToArray());
            }
        }
    }
}
