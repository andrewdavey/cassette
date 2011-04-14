using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Knapsack
{
    public class Script
    {
        private string path;
        private byte[] hash;
        private string[] references;

        public Script(string path, byte[] hash, string[] references)
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
