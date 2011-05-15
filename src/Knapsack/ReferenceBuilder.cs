using System;
using System.Linq;
using System.Collections.Generic;
using Knapsack.Utilities;

namespace Knapsack
{
    public class ReferenceBuilder : IReferenceBuilder
    {
        readonly ModuleContainer moduleContainer;
        readonly HashSet<Module> modules = new HashSet<Module>();

        public ReferenceBuilder(ModuleContainer moduleContainer)
        {
            this.moduleContainer = moduleContainer;
        }

        public void AddReference(string resourcePath)
        {
            var module = moduleContainer.FindModuleContainingResource(resourcePath);
            if (module == null) throw new ArgumentException("Resource not found: " + resourcePath);
            modules.Add(module);
        }

        public IEnumerable<Module> GetRequiredModules()
        {
            // Get current modules since we will add more to the set.
            // It's not valid to change a collection during enumeration.
            var currentModules = modules.ToArray(); 
            foreach (var module in currentModules)
            {
                AddReferencedModules(module);
            }
            return OrderModulesByDependency(modules);
        }

        void AddReferencedModules(Module module)
        {
            foreach (var reference in module.References)
            {
                var referencedModule = moduleContainer.FindModule(reference);
                if (!modules.Contains(referencedModule))
                {
                    modules.Add(referencedModule);
                    AddReferencedModules(referencedModule);
                }
            }
        }

        IEnumerable<Module> OrderModulesByDependency(IEnumerable<Module> modules)
        {
            var modulesByPath = modules.ToDictionary(m => m.Path, StringComparer.OrdinalIgnoreCase);

            var graph = new Graph<string>(
                modules.Select(m => m.Path),
                path => modulesByPath[path].References
            );

            return graph.TopologicalSort().Select(path => modulesByPath[path]);
        }
    }
}
