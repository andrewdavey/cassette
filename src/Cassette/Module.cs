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
        readonly List<IAsset> assets = new List<IAsset>();

        public string Directory
        {
            get { return directory; }
        }

        public IList<IAsset> Assets
        {
            get { return assets; }
        }
    }
}
