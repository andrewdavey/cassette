using System;

namespace Cassette
{
    public class ScriptModuleFactory : IModuleFactory<ScriptModule>
    {
        public ScriptModuleFactory(Func<string, string> getFullPath)
        {
            this.getFullPath = getFullPath;
        }

        readonly Func<string, string> getFullPath;

        public ScriptModule CreateModule(string directory)
        {
            return new ScriptModule(directory, getFullPath);
        }
    }
}