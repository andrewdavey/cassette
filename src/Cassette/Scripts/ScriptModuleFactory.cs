using System.Linq;
using Cassette.Utilities;

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

        public ScriptModule CreateExternalModule(string name, ModuleDescriptor moduleDescriptor)
        {
            var module = new ExternalScriptModule(name, moduleDescriptor.ExternalUrl);
            if (moduleDescriptor.FallbackCondition != null)
            {
                var assets = moduleDescriptor.AssetFilenames.Select(
                    filename => new Asset(
                        PathUtilities.CombineWithForwardSlashes(name, filename),
                        module,
                        moduleDescriptor.SourceFile.Directory.GetFile(filename)
                    )
                );
                module.AddFallback(moduleDescriptor.FallbackCondition, assets);
            }
            return module;
        }
    }
}