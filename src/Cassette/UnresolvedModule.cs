using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Cassette.Utilities;
using System.IO;

namespace Cassette
{
    /// <summary>
    /// An unresolved module exists while the module dependency graph is being built.
    /// </summary>
    public class UnresolvedModule
    {
        readonly string path;
        readonly Tuple<Asset, string[]>[] partition;
        readonly Asset[] assets;
        readonly string[] externalReferences;
        readonly string location;

        /// <param name="path">Application relative path to the module directory.</param>
        /// <param name="assets">All the unresolved assets found in the module.</param>
        /// <param name="isAssetOrderFixed">When true, the assets will not be sorted by their dependency ordering.</param>
        public UnresolvedModule(string path, UnresolvedAsset[] assets, string location, bool isAssetOrderFixed)
        {
            this.path = path;
            this.location = location;

            var pathsInModule = new HashSet<string>(assets.Select(asset => asset.Path));
            partition = PartitionAssetReferences(assets, pathsInModule);
            // Store all the references to external assets.
            this.externalReferences = partition.SelectMany(p => p.Item2).Distinct().ToArray();
            // The assets now only contain references found in this module.
            var resolvedAssets = partition.Select(p => p.Item1).ToArray();
            if (isAssetOrderFixed)
            {
                this.assets = resolvedAssets;
            }
            else
            {
                this.assets = OrderScriptsByDependency(resolvedAssets);
            }
        }

        public Module Resolve(Func<string, string> getModulePathContainingAsset)
        {
            var moduleReferences = externalReferences
                .Select(
                    externalPath => getModulePathContainingAsset(externalPath)
                )
                .Distinct()
                .ToArray();

            return new Module(path, assets, moduleReferences, location);
        }

        public static IEnumerable<Module> ResolveAll(IEnumerable<UnresolvedModule> unresolvedModules)
        {
            unresolvedModules = unresolvedModules.Where(m => m.assets.Length > 0).ToArray();

            var modulesByAssetPath = (
                from module in unresolvedModules
                from asset in module.assets
                select new { asset.Path, module }
            ).ToDictionary(x => x.Path, x => x.module.path, StringComparer.OrdinalIgnoreCase);

            var modules = unresolvedModules.Select(
                m => m.Resolve(
                    assetPath => FindModulePathOrThrow(modulesByAssetPath, m, assetPath)
                )
            );

            return modules;
        }

        static string FindModulePathOrThrow(Dictionary<string, string> modulesByAssetPath, UnresolvedModule m, string assetPath)
        {
            string module;
            if (modulesByAssetPath.TryGetValue(assetPath, out module))
            {
                return module;
            }
            else
            {
                var referencers = string.Join(
                    "\", \"",
                    m.GetAssetPathsThatReferencePath(assetPath)
                );
                throw new FileNotFoundException(
                    string.Format(
                        "The file \"{0}\" is referenced by \"{1}\", but cannot be found. " + 
                        "Either add \"{0}\" or change the reference(s) to a file that exists.", 
                        assetPath, 
                        referencers
                    ), 
                    assetPath
                );
            }
        }

        IEnumerable<string> GetAssetPathsThatReferencePath(string assetPath)
        {
            return partition
                .Where(p => p.Item2.Contains(assetPath))
                .Select(p => p.Item1.Path);
        }

        Tuple<Asset, string[]>[] PartitionAssetReferences(UnresolvedAsset[] assets, HashSet<string> pathsInModule)
        {
            return assets.Select(
                asset => asset.Resolve(pathsInModule.Contains)
            ).ToArray();
        }

        Asset[] OrderScriptsByDependency(Asset[] assets)
        {
            var assetsByPath = assets.ToDictionary(asset => asset.Path, StringComparer.OrdinalIgnoreCase);
            // Create a graph where each node is a asset path
            // and directed edges represent references between assets.
            var graph = new Graph<string>(
                assetsByPath.Keys, 
                path => assetsByPath[path].References
            );
            var sortedPaths = graph.TopologicalSort();

            return sortedPaths.Select(path => assetsByPath[path]).ToArray();
        }
    }
}
