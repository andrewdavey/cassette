using System;
using System.Linq;
using System.Collections.Generic;

namespace Cassette
{
    public class ReferenceBuilder<T> : IReferenceBuilder<T>
        where T: Module
    {
        public ReferenceBuilder(IModuleContainer<T> moduleContainer, IModuleFactory<T> moduleFactory)
        {
            this.moduleContainer = moduleContainer;
            this.moduleFactory = moduleFactory;
        }

        readonly IModuleContainer<T> moduleContainer;
        readonly IModuleFactory<T> moduleFactory;
        readonly HashSet<T> modules = new HashSet<T>();

        public void AddReference(string path)
        {
            if (IsUrl(path))
            {
                modules.Add(moduleFactory.CreateExternalModule(path));
            }
            else
            {
                var module = moduleContainer.FindModuleByPath(path);
                if (module == null)
                {
                    throw new ArgumentException("Cannot find an asset module containing the path \"" + path + "\".");
                }
                modules.Add(module);
            }
        }

        bool IsUrl(string path)
        {
            return path.StartsWith("http:", StringComparison.OrdinalIgnoreCase)
                || path.StartsWith("https:", StringComparison.OrdinalIgnoreCase)
                || path.StartsWith("//");
        }

        public IEnumerable<T> GetModules(string location)
        {
            var modulesAtLocation = modules.Where(m => m.Location == location).ToArray();
            return moduleContainer.AddDependenciesAndSort(modulesAtLocation);
        }
    }
}
