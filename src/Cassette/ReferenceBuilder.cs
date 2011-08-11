using System;
using System.Linq;
using System.Collections.Generic;

namespace Cassette
{
    public class ReferenceBuilder<T> : IReferenceBuilder<T>
        where T: Module
    {
        public ReferenceBuilder(IModuleContainer<T> moduleContainer)
        {
            this.moduleContainer = moduleContainer;
        }

        readonly IModuleContainer<T> moduleContainer;
        readonly HashSet<T> modules = new HashSet<T>();

        public void AddReference(string path)
        {
            var module = moduleContainer.FindModuleByPath(path);
            if (module == null)
            {
                throw new ArgumentException("Cannot find an asset module containing the path \"" + path + "\".");
            }
            modules.Add(module);
        }

        public IEnumerable<T> GetModules(string location)
        {
            var modulesAtLocation = modules.Where(m => m.Location == location).ToArray();
            return moduleContainer.AddDependenciesAndSort(modulesAtLocation);
        }
    }
}
