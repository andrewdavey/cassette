using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Cassette
{
    public class ScriptModule : Module
    {
        public ScriptModule(string directory, IFileSystem fileSystem)
            : base(directory, fileSystem)
        {
        }
    }
}
