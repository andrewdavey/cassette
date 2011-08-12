using System;

namespace Cassette
{
    public class ScriptModuleFactory : IModuleFactory<ScriptModule>
    {
        public ScriptModuleFactory(IFileSystem fileSystem)
        {
            this.fileSystem = fileSystem;
        }

        readonly IFileSystem fileSystem;

        public ScriptModule CreateModule(string directory)
        {
            return new ScriptModule(directory, fileSystem);
        }

        public ScriptModule CreateExternalModule(string url)
        {
            return new ExternalScriptModule(url);
        }
    }
}