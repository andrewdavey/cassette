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
            ValidateAssetReferences();
            moduleImmediateReferences = BuildModuleImmediateReferenceDictionary();
            sortIndex = BuildSortIndex();
        }

        readonly IEnumerable<T> modules;
        readonly Dictionary<T, HashSet<T>> moduleImmediateReferences;
        readonly Dictionary<T, int> sortIndex;

        public IEnumerable<T> Modules
        {
            get { return modules; }
        }

        public IEnumerable<T> AddDependenciesAndSort(IEnumerable<T> modules)
        {
            var references = new HashSet<T>(modules);
            foreach (var module in modules)
            {
                AddModulesReferencedBy(module, references);
            }
            return references.OrderBy(m => sortIndex[m]);
        }

        public T FindModuleByPath(string path)
        {
            return modules.FirstOrDefault(module => module.ContainsPath(path));
        }

        void ValidateAssetReferences()
        {
            var notFound = from module in modules
                           from asset in module.Assets
                           from reference in asset.References
                           where reference.Type == AssetReferenceType.DifferentModule
                              && modules.Any(m => m.ContainsPath(reference.ReferencedFilename)) == false
                           select CreateAssetReferenceNotFoundMessage(reference);

            var message = string.Join(Environment.NewLine, notFound);
            if (message.Length > 0)
            {
                throw new AssetReferenceException(message);
            }
        }

        Dictionary<T, HashSet<T>> BuildModuleImmediateReferenceDictionary()
        {
            return (
                from module in modules
                select new 
                { 
                    module, 
                    references = new HashSet<T>(module.Assets.SelectMany(a => a.References)
                        .Where(r => r.Type == AssetReferenceType.DifferentModule)
                        .Select(r => FindModuleByPath(r.ReferencedFilename))
                    ) 
                }
            ).ToDictionary(x => x.module, x => x.references);
        }

        Dictionary<T, int> BuildSortIndex()
        {
            var graph = new Graph<T>(modules, m => moduleImmediateReferences[m]);
            return graph.TopologicalSort()
                .Select((module, index) => new { module, index })
                .ToDictionary(x => x.module, x => x.index);
        }

        string CreateAssetReferenceNotFoundMessage(AssetReference reference)
        {
            if (reference.SourceLineNumber > 0)
            {
                return string.Format(
                    "Reference error in \"{0}\", line {1}. Cannot find \"{2}\".",
                    reference.SourceAsset.SourceFilename, reference.SourceLineNumber, reference.ReferencedFilename
                );
            }
            else
            {
                return string.Format(
                    "Reference error in \"{0}\". Cannot find \"{1}\".",
                    reference.SourceAsset.SourceFilename, reference.ReferencedFilename
                );
            }
        }

        void AddModulesReferencedBy(T module, HashSet<T> references)
        {
            var referencedModules = moduleImmediateReferences[module];
            foreach (var referencedModule in referencedModules)
            {
                AddModulesReferencedBy(referencedModule, references);
                references.Add(referencedModule);
            }
        }
    }
}
