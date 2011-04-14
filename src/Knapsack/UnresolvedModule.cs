using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Knapsack
{
    /// <summary>
    /// An unresolved module exists while the module dependency graph is being built.
    /// </summary>
    public class UnresolvedModule
    {
        readonly string path;
        readonly Script[] scripts;
        readonly string[] externalScriptReferences;

        public UnresolvedModule(string path, Script[] scripts)
        {
            if (System.IO.Path.IsPathRooted(path) == false) throw new ArgumentException("Absolute path required.", "path");
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

        Tuple<Script, string[]>[] PartitionScriptReferences(Script[] scripts, HashSet<string> pathsInModule)
        {
            return scripts.Select(
                s => s.ExtractExternalReferences(pathsInModule.Contains)
            ).ToArray();
        }

        Script[] OrderScriptsByDependency(Script[] scripts)
        {
            var scriptsByPath = scripts.ToDictionary(s => s.Path);
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
