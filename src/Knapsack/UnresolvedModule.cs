using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Knapsack.Utilities;

namespace Knapsack
{
    /// <summary>
    /// An unresolved module exists while the module dependency graph is being built.
    /// </summary>
    public class UnresolvedModule
    {
        readonly string path;
        readonly Resource[] resources;
        readonly string[] externalReferences;

        /// <param name="path">Application relative path to the module directory.</param>
        /// <param name="resources">All the unresolved resources found in the module.</param>
        /// <param name="isResourceOrderFixed">When true, the resources will not be sorted by their dependency ordering.</param>
        public UnresolvedModule(string path, UnresolvedResource[] resources, bool isResourceOrderFixed)
        {
            this.path = path;

            var pathsInModule = new HashSet<string>(resources.Select(resource => resource.Path));
            var partition = PartitionResourceReferences(resources, pathsInModule);
            // Store all the references to external resources.
            this.externalReferences = partition.SelectMany(p => p.Item2).Distinct().ToArray();
            // The resources now only contain references found in this module.
            var resolvedResources = partition.Select(p => p.Item1).ToArray();
            if (isResourceOrderFixed)
            {
                this.resources = resolvedResources;
            }
            else
            {
                this.resources = OrderScriptsByDependency(resolvedResources);
            }
        }

        public Module Resolve(Func<string, string> getModulePathContainingResource)
        {
            var moduleReferences = externalReferences
                .Select(
                    externalPath => getModulePathContainingResource(externalPath)
                )
                .Distinct()
                .ToArray();

            return new Module(path, resources, moduleReferences);
        }

        public static IEnumerable<Module> ResolveAll(IEnumerable<UnresolvedModule> unresolvedModules)
        {
            unresolvedModules = unresolvedModules.Where(m => m.resources.Length > 0);

            var modulesByResourcePath = (
                from module in unresolvedModules
                from resource in module.resources
                select new { resource.Path, module }
            ).ToDictionary(x => x.Path, x => x.module.path, StringComparer.OrdinalIgnoreCase);

            var modules = unresolvedModules.Select(
                m => m.Resolve(resourcePath => modulesByResourcePath[resourcePath])
            );

            return modules;
        }

        Tuple<Resource, string[]>[] PartitionResourceReferences(UnresolvedResource[] resources, HashSet<string> pathsInModule)
        {
            return resources.Select(
                resource => resource.Resolve(pathsInModule.Contains)
            ).ToArray();
        }

        Resource[] OrderScriptsByDependency(Resource[] resources)
        {
            var resourcesByPath = resources.ToDictionary(resource => resource.Path, StringComparer.OrdinalIgnoreCase);
            // Create a graph where each node is a resource path
            // and directed edges represent references between resources.
            var graph = new Graph<string>(
                resourcesByPath.Keys, 
                path => resourcesByPath[path].References
            );
            var sortedPaths = graph.TopologicalSort();

            return sortedPaths.Select(path => resourcesByPath[path]).ToArray();
        }
    }
}
