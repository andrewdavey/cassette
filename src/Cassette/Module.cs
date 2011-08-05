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

        public bool Contains(string path)
        {
            return path.StartsWith(directory, StringComparison.OrdinalIgnoreCase);
        }
    }
}
