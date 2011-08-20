using System;
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
        readonly Dictionary<string, HashSet<T>> modulesByLocation = new Dictionary<string, HashSet<T>>();
        
        public void AddReference(string path, string location)
        {
            if (IsUrl(path))
            {
                var modules = GetOrCreateModuleSet(location);
                modules.Add(moduleFactory.CreateExternalModule(path));
            }
            else
            {
                var referencedModule = moduleContainer.FindModuleContainingPath(path);
                if (referencedModule == null)
                {
                    throw new ArgumentException("Cannot find an asset module containing the path \"" + path + "\".");
                }
                var allRequiredModules = moduleContainer.ConcatDependencies(referencedModule);
                foreach (var module in allRequiredModules)
                {
                    AddReferenceToModule(module, location);
                }
            }
        }

        void AddReferenceToModule(T module, string location)
        {
            // Module can define it's own prefered location. Use this when we aren't given
            // an explicit location argument i.e. null.
            if (location == null)
            {
                location = module.Location;
            }
            var moduleSet = GetOrCreateModuleSet(location);
            moduleSet.Add(module);
        }

        public IEnumerable<T> GetModules(string location)
        {
            var modules = GetOrCreateModuleSet(location);
            return moduleContainer.SortModules(modules);
        }

        HashSet<T> GetOrCreateModuleSet(string location)
        {
            location = location ?? ""; // Dictionary doesn't accept null keys.
            HashSet<T> modules;
            if (modulesByLocation.TryGetValue(location, out modules))
            {
                return modules;
            }
            else
            {
                modules = new HashSet<T>();
                modulesByLocation.Add(location, modules);
                return modules;
            }
        }

        bool IsUrl(string path)
        {
            return path.StartsWith("http:", StringComparison.OrdinalIgnoreCase)
                || path.StartsWith("https:", StringComparison.OrdinalIgnoreCase)
                || path.StartsWith("//");
        }
    }
}
