using System;
using System.Collections.Generic;

namespace Cassette
{
    public class ReferenceBuilder<T>
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

        public IEnumerable<T> GetModules()
        {
            return moduleContainer.AddDependenciesAndSort(modules);
        }
    }
}
