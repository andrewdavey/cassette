using System;
using System.Linq;

namespace Knapsack
{
    public class Script
    {
        readonly string path;
        readonly byte[] hash;
        readonly string[] references;

        public Script(string path, byte[] hash, string[] references)
        {
            if (System.IO.Path.IsPathRooted(path) == false) throw new ArgumentException("Absolute path required.", "path");

            this.path = path;
            this.hash = hash;
            this.references = references;
        }

        public string Path
        {
            get { return path; }
        }

        public byte[] Hash
        {
            get { return hash; }
        }

        public string[] References
        {
            get { return references; }
        }

        public Tuple<Script, string[]> ExtractExternalReferences(Func<string, bool> isInternalPath)
        {
            var newScript = new Script(path, hash, references.Where(isInternalPath).ToArray());
            var externalReferences = references.Where(r => !isInternalPath(r)).ToArray();
            return Tuple.Create(newScript, externalReferences);
        }
    }
}
