#region License
/*
Copyright 2011 Andrew Davey

This file is part of Cassette.

Cassette is free software: you can redistribute it and/or modify it under the 
terms of the GNU General Public License as published by the Free Software 
Foundation, either version 3 of the License, or (at your option) any later 
version.

Cassette is distributed in the hope that it will be useful, but WITHOUT ANY 
WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS 
FOR A PARTICULAR PURPOSE. See the GNU General Public License for more details.

You should have received a copy of the GNU General Public License along with 
Cassette. If not, see http://www.gnu.org/licenses/.
*/
#endregion

using System;
using System.Collections.Generic;
using System.Linq;
using Cassette.Utilities;

namespace Cassette
{
    public class BundleContainer : IBundleContainer
    {
        public BundleContainer(IEnumerable<Bundle> bundles)
        {
            this.bundles = bundles.ToArray(); // Force eval to prevent repeatedly generating new bundles.

            ValidateBundleReferences();
            ValidateAssetReferences();
            bundleImmediateReferences = BuildBundleImmediateReferenceDictionary();
        }

        readonly Bundle[] bundles;
        readonly Dictionary<Bundle, HashSet<Bundle>> bundleImmediateReferences;

        public IEnumerable<Bundle> Bundles
        {
            get { return bundles; }
        }

        public IEnumerable<Bundle> IncludeReferencesAndSortBundles(IEnumerable<Bundle> bundlesToSort)
        {
            var bundlesArray = bundlesToSort.ToArray();
            var references = GetBundleReferencesWithImplicitOrderingIncluded(bundlesArray);
            var all = new HashSet<Bundle>();
            foreach (var bundle in bundlesArray)
            {
                AddBundlesReferencedBy(bundle, all);   
            }
            var graph = BuildBundleGraph(references, all);
            var cycles = graph.FindCycles().ToArray();
            if (cycles.Length > 0)
            {
                var details = string.Join(Environment.NewLine, cycles.Select(cycle => "[" + string.Join(", ", cycle.Select(m => m.Path)) + "]"));
                throw new InvalidOperationException(
                    "Cycles detected in bundle dependency graph:" + Environment.NewLine +
                    details
                );
            }
            return graph.TopologicalSort();
        }

        Graph<Bundle> BuildBundleGraph(IDictionary<Bundle, HashSet<Bundle>> references, IEnumerable<Bundle> all)
        {
            return new Graph<Bundle>(
                all,
                bundle =>
                {
                    HashSet<Bundle> set;
                    if (references.TryGetValue(bundle, out set)) return set;
                    return Enumerable.Empty<Bundle>();
                }
            );
        }

        Dictionary<Bundle, HashSet<Bundle>> GetBundleReferencesWithImplicitOrderingIncluded(IEnumerable<Bundle> initialBundles)
        {
            var roots = initialBundles.Where(m =>
            {
                HashSet<Bundle> set;
                if (bundleImmediateReferences.TryGetValue(m, out set)) return set.Count == 0;
                return true;
            }).ToList();

            // Clone the original references dictionary, so we can add the extra
            // implicit references based on array order.
            var references = new Dictionary<Bundle, HashSet<Bundle>>();
            foreach (var reference in bundleImmediateReferences)
            {
                references[reference.Key] = new HashSet<Bundle>(reference.Value);
            }
            for (int i = 1; i < roots.Count; i++)
            {
                var bundle = roots[i];
                var previous = roots[i - 1];
                HashSet<Bundle> set;
                if (!references.TryGetValue(bundle, out set))
                {
                    references[bundle] = set = new HashSet<Bundle>();
                }
                set.Add(previous);
            }
            return references;
        }

        void AddBundlesReferencedBy(Bundle bundle, ISet<Bundle> all)
        {
            if (all.Contains(bundle)) return;
            all.Add(bundle);

            HashSet<Bundle> referencedBundles;
            if (!bundleImmediateReferences.TryGetValue(bundle, out referencedBundles)) return;
            foreach (var referencedBundle in referencedBundles)
            {
                AddBundlesReferencedBy(referencedBundle, all);
            }
        }

        public Bundle FindBundleContainingPath(string path)
        {
            return bundles.FirstOrDefault(bundle => bundle.ContainsPath(path));
        }

        void ValidateBundleReferences()
        {
            var notFound = from bundle in bundles
                           from reference in bundle.References
                           where bundles.Any(m => m.ContainsPath(reference)) == false
                           select string.Format(
                               "Reference error in bundle descriptor for \"{0}\". Cannot find \"{1}\".",
                               bundle.Path,
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
            var notFound = from bundle in bundles
                           from asset in bundle.Assets
                           from reference in asset.References
                           where reference.Type == AssetReferenceType.DifferentBundle
                              && bundles.Any(m => m.ContainsPath(reference.Path)) == false
                           select CreateAssetReferenceNotFoundMessage(reference);

            var message = string.Join(Environment.NewLine, notFound);
            if (message.Length > 0)
            {
                throw new AssetReferenceException(message);
            }
        }

        Dictionary<Bundle, HashSet<Bundle>> BuildBundleImmediateReferenceDictionary()
        {
            return (
                from bundle in bundles
                select new 
                { 
                    bundle,
                    references = new HashSet<Bundle>(bundle.Assets.SelectMany(a => a.References)
                        .Where(r => r.Type == AssetReferenceType.DifferentBundle
                                 || r.Type == AssetReferenceType.Url)
                        .Select(r => r.Path)
                        .Concat(bundle.References)
                        .Select(FindBundleContainingPath)
                    ) 
                }
            ).ToDictionary(x => x.bundle, x => x.references);
        }

        string CreateAssetReferenceNotFoundMessage(AssetReference reference)
        {
            if (reference.SourceLineNumber > 0)
            {
                return string.Format(
                    "Reference error in \"{0}\", line {1}. Cannot find \"{2}\".",
                    reference.SourceAsset.SourceFile.FullPath, reference.SourceLineNumber, reference.Path
                );
            }
            else
            {
                return string.Format(
                    "Reference error in \"{0}\". Cannot find \"{1}\".",
                    reference.SourceAsset.SourceFile.FullPath, reference.Path
                );
            }
        }

        public void Dispose()
        {
            foreach (IDisposable bundle in bundles)
            {
                bundle.Dispose();
            }
        }
    }
}