using System;
using System.Linq;
using System.Collections.Generic;
using Cassette.Utilities;

namespace Cassette
{
    public class ReferenceBuilder : IReferenceBuilder
    {
        readonly ModuleContainer moduleContainer;
        readonly HashSet<Module> modules = new HashSet<Module>();

        public ReferenceBuilder(ModuleContainer moduleContainer)
        {
            this.moduleContainer = moduleContainer;
        }

        public ModuleContainer ModuleContainer
        {
            get { return moduleContainer; }
        }

        public void AddReference(string assetPath)
        {
            var module = moduleContainer.FindModuleContainingAsset(assetPath)
                      ?? moduleContainer.FindModule(assetPath.TrimEnd('/', '*'));
            if (module == null)
            {
                // The assetPath may be an external URL.
                Uri url;
                if (Uri.TryCreate(assetPath, UriKind.Absolute, out url))
                {
                    modules.Add(Module.CreateExternalModule(assetPath, location: ""));
                }
                else
                {
                    throw new ArgumentException("Asset not found: " + assetPath);
                }
            }
            else
            {
                modules.Add(module);
            }
        }

        public void AddExternalReference(string externalUrl, string location)
        {
            Uri url;
            if (Uri.TryCreate(externalUrl, UriKind.Absolute, out url))
            {
                modules.Add(Module.CreateExternalModule(externalUrl, location));
            }
            else
            {
                throw new ArgumentException("External URL must be an absolute URI.", "externalUrl");
            }
        }

        public IEnumerable<Module> GetRequiredModules()
        {
            // By convention, if an application wide module has been configured
            // then we always require it, even if no pages have explicitly added it
            // as reference. This avoids a confusing call to Assets.Scripts.Reference("").
            if (IsSingleApplicationWideModuleRequired())
            {
                return new[] { moduleContainer.FindModule("") };
            }

            // Get current modules since we will add more to the set.
            // It's not valid to change a collection during enumeration.
            var currentModules = modules.ToArray();
            foreach (var module in currentModules)
            {
                AddReferencedModules(module);
            }

            return OrderModulesByDependency(modules);
        }

        bool IsSingleApplicationWideModuleRequired()
        {
            return modules.Where(m => m.IsExternal == false).Any() == false
                && moduleContainer.Contains("");
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
