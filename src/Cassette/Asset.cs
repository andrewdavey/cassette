using System;
using System.Linq;

namespace Cassette
{
    public class Asset
    {
        readonly string path;
        readonly byte[] hash;
        readonly string[] references;

        public Asset(string path, byte[] hash, string[] references)
        {
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
    }
}
