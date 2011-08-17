using System;

namespace Cassette.Scripts
{
    public class ScriptModuleFactory : IModuleFactory<ScriptModule>
    {
        public ScriptModule CreateModule(string directory)
        {
            return new ScriptModule(directory);
        }

        public ScriptModule CreateExternalModule(string url)
        {
            return new ExternalScriptModule(url);
        }
    }
}