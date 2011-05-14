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
        readonly Resource[] scripts;
        readonly string[] externalScriptReferences;

        public UnresolvedModule(string path, UnresolvedResource[] scripts)
        {
            this.path = path;

            var pathsInModule = new HashSet<string>(scripts.Select(s => s.Path));
            var partition = PartitionScriptReferences(scripts, pathsInModule);
            // Store all the references to external scripts.
            this.externalScriptReferences = partition.SelectMany(p => p.Item2).Distinct().ToArray();
            // The scripts now only contain references found in this module.
            this.scripts = OrderScriptsByDependency(partition.Select(p => p.Item1).ToArray());
        }

        public Module Resolve(Func<string, string> getModulePathContainingScript)
        {
            var moduleReferences = externalScriptReferences
                .Select(
                    p => getModulePathContainingScript(p)
                )
                .Distinct()
                .ToArray();

            return new Module(path, scripts, moduleReferences);
        }

        public static IEnumerable<Module> ResolveAll(IEnumerable<UnresolvedModule> unresolvedModules)
        {
            var modulesByScriptPath = (
                from module in unresolvedModules
                from script in module.scripts
                select new { script.Path, module }
            ).ToDictionary(x => x.Path, x => x.module.path, StringComparer.OrdinalIgnoreCase);

            var modules = unresolvedModules.Select(
                m => m.Resolve(scriptPath => modulesByScriptPath[scriptPath])
            );

            return modules;
        }

        Tuple<Resource, string[]>[] PartitionScriptReferences(UnresolvedResource[] scripts, HashSet<string> pathsInModule)
        {
            return scripts.Select(
                s => s.Resolve(pathsInModule.Contains)
            ).ToArray();
        }

        Resource[] OrderScriptsByDependency(Resource[] scripts)
        {
            var scriptsByPath = scripts.ToDictionary(s => s.Path, StringComparer.OrdinalIgnoreCase);
            // Create a graph where each node is a script path
            // and directed edges represent references between scripts.
            var graph = new Graph<string>(
                scriptsByPath.Keys, 
                path => scriptsByPath[path].References
            );
            var sortedPaths = graph.TopologicalSort();

            return sortedPaths.Select(path => scriptsByPath[path]).ToArray();
        }
    }
}
