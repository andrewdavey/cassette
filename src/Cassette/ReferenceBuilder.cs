using System;
using System.Collections.Generic;
using Cassette.Utilities;

namespace Cassette
{
    public class ReferenceBuilder<T> : IReferenceBuilder
        where T: Module
    {
        public ReferenceBuilder(IModuleContainer<T> moduleContainer, IModuleFactory<T> moduleFactory)
        {
            this.moduleContainer = moduleContainer;
            this.moduleFactory = moduleFactory;
        }

        readonly IModuleContainer<T> moduleContainer;
        readonly IModuleFactory<T> moduleFactory;
        readonly Dictionary<string, List<Module>> modulesByLocation = new Dictionary<string, List<Module>>();
        
        public void AddReference(string path, string location)
        {
            path = PathUtilities.AppRelative(path);

            var module = moduleContainer.FindModuleContainingPath(path);
            if (module == null && path.IsUrl())
            {
                // Ad-hoc external module reference.
                module = moduleFactory.CreateExternalModule(path);
            }

            if (module == null)
            {
                throw new ArgumentException("Cannot find an asset module containing the path \"" + path + "\".");                
            }

            // Module can define it's own prefered location. Use this when we aren't given
            // an explicit location argument i.e. null.
            if (location == null)
            {
                location = module.Location;
            }

            AddReference(module, location);
        }

        public void AddReference(Module module, string location)
        {
            var modules = GetOrCreateModuleSet(location);
            if (modules.Contains(module)) return;
            modules.Add(module);
        }

        public IEnumerable<Module> GetModules(string location)
        {
            var modules = GetOrCreateModuleSet(location);
            return moduleContainer.IncludeReferencesAndSortModules(modules);
        }

        List<Module> GetOrCreateModuleSet(string location)
        {
            location = location ?? ""; // Dictionary doesn't accept null keys.
            List<Module> modules;
            if (modulesByLocation.TryGetValue(location, out modules))
            {
                return modules;
            }
            else
            {
                modules = new List<Module>();
                modulesByLocation.Add(location, modules);
                return modules;
            }
        }
    }
}