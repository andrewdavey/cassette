using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Cassette
{
    public class ScriptModuleFactory : IModuleFactory<ScriptModule>
    {
        public ScriptModuleFactory(Func<string, string> getFullPath)
        {
            this.getFullPath = getFullPath;
        }

        readonly Func<string, string> getFullPath;

        public ScriptModule CreateModule(string directoryPath)
        {
            return new ScriptModule(directoryPath, getFullPath);
        }
    }
}
