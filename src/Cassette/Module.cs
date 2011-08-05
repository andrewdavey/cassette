using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Cassette
{
    public class Module
    {
        public Module(string directory)
        {
            this.directory = directory;
        }

        readonly string directory;
        readonly List<Asset> assets = new List<Asset>();

        public string Directory
        {
            get { return directory; }
        }

        public List<Asset> Assets
        {
            get { return assets; }
        }
    }
}
