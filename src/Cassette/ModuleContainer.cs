using System;
using System.Collections.Generic;
using System.Linq;
using Cassette.Utilities;

namespace Cassette
{
    public class ModuleContainer<T> : IModuleContainer<T>
        where T : Module
    {
        public ModuleContainer(IEnumerable<T> modules)
        {
            this.modules = modules.ToArray(); // Force eval to prevent repeatedly generating new modules.

            ValidateModuleReferences();
            ValidateAssetReferences();
            moduleImmediateReferences = BuildModuleImmediateReferenceDictionary();
        }

        readonly T[] modules;
        readonly Dictionary<Module, HashSet<Module>> moduleImmediateReferences;

        public IEnumerable<T> Modules
        {
            get { return modules; }
        }

        public IEnumerable<Module> IncludeReferencesAndSortModules(IEnumerable<Module> modulesToSort)
        {
            var modulesArray = modulesToSort.ToArray();
            var references = GetModuleReferencesWithImplicitOrderingIncluded(modulesArray);
            var all = new HashSet<Module>();
            foreach (var module in modulesArray)
            {
                AddModulesReferencedBy(module, all);   
            }
            var graph = BuildModuleGraph(references, all);
            var cycles = graph.FindCycles().ToArray();
            if (cycles.Length > 0)
            {
                var details = string.Join(Environment.NewLine, "[" + cycles.Select(cycle => string.Join(", ", cycle.Select(m => m.Path))) + "]");
                throw new InvalidOperationException(
                    "Cycles detected in module dependency graph:" + Environment.NewLine +
                    details
                );
            }
            return graph.TopologicalSort();
        }

        Graph<Module> BuildModuleGraph(IDictionary<Module, HashSet<Module>> references, IEnumerable<Module> all)
        {
            return new Graph<Module>(
                all,
                module =>
                {
                    HashSet<Module> set;
                    if (references.TryGetValue(module, out set)) return set;
                    return Enumerable.Empty<T>();
                }
            );
        }

        Dictionary<Module, HashSet<Module>> GetModuleReferencesWithImplicitOrderingIncluded(IEnumerable<Module> modules)
        {
            var roots = modules.Where(m =>
            {
                HashSet<Module> set;
                if (moduleImmediateReferences.TryGetValue(m, out set)) return set.Count == 0;
                return true;
            }).ToList();

            // Clone the original references dictionary, so we can add the extra
            // implicit references based on array order.
            var references = new Dictionary<Module, HashSet<Module>>();
            foreach (var reference in moduleImmediateReferences)
            {
                references[reference.Key] = new HashSet<Module>(reference.Value);
            }
            for (int i = 1; i < roots.Count; i++)
            {
                var module = roots[i];
                var previous = roots[i - 1];
                HashSet<Module> set;
                if (!references.TryGetValue(module, out set))
                {
                    references[module] = set = new HashSet<Module>();
                }
                set.Add(previous);
            }
            return references;
        }

        void AddModulesReferencedBy(Module module, HashSet<Module> all)
        {
            if (all.Contains(module)) return;
            all.Add(module);

            HashSet<Module> referencedModules;
            if (!moduleImmediateReferences.TryGetValue(module, out referencedModules)) return;
            foreach (var referencedModule in referencedModules)
            {
                AddModulesReferencedBy(referencedModule, all);
            }
        }

        public T FindModuleContainingPath(string path)
        {
            return modules.FirstOrDefault(module => module.ContainsPath(path));
        }

        void ValidateModuleReferences()
        {
            var notFound = from module in modules
                           from reference in module.References
                           where modules.Any(m => m.ContainsPath(reference)) == false
                           select string.Format(
                               "Reference error in module descriptor for \"{0}\". Cannot find \"{1}\".",
                               module.Path,
                               reference
                           );
            var message = string.Join(Environment.NewLine, notFound);
            if (message.Length > 0)
            {
                throw new AssetReferenceException(message);
            }
        }

        void ValidateAssetReferences()
        {
            var notFound = from module in modules
                           from asset in module.Assets
                           from reference in asset.References
                           where reference.Type == AssetReferenceType.DifferentModule
                              && modules.Any(m => m.ContainsPath(reference.Path)) == false
                           select CreateAssetReferenceNotFoundMessage(reference);

            var message = string.Join(Environment.NewLine, notFound);
            if (message.Length > 0)
            {
                throw new AssetReferenceException(message);
            }
        }

        Dictionary<Module, HashSet<Module>> BuildModuleImmediateReferenceDictionary()
        {
            return (
                from module in modules
                select new 
                { 
                    module,
                    references = new HashSet<Module>(module.Assets.SelectMany(a => a.References)
                        .Where(r => r.Type == AssetReferenceType.DifferentModule
                                 || r.Type == AssetReferenceType.Url)
                        .Select(r => r.Path)
                        .Concat(module.References)
                        .Select(FindModuleContainingPath)
                    ) 
                }
            ).ToDictionary(x => (Module)x.module, x => x.references);
        }

        string CreateAssetReferenceNotFoundMessage(AssetReference reference)
        {
            if (reference.SourceLineNumber > 0)
            {
                return string.Format(
                    "Reference error in \"{0}\", line {1}. Cannot find \"{2}\".",
                    reference.SourceAsset.SourceFilename, reference.SourceLineNumber, reference.Path
                );
            }
            else
            {
                return string.Format(
                    "Reference error in \"{0}\". Cannot find \"{1}\".",
                    reference.SourceAsset.SourceFilename, reference.Path
                );
            }
        }

        public void Dispose()
        {
            foreach (var module in modules)
            {
                module.Dispose();
            }
        }
    }
}
