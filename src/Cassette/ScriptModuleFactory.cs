using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Cassette
{
    public class ScriptModuleFactory : IModuleFactory<ScriptModule>
    {
        public ScriptModule CreateModule(string directoryPath)
        {
            return new ScriptModule(directoryPath);
        }
    }
}
