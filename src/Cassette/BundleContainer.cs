using System;
using System.Collections.Generic;
using System.Linq;
using Cassette.Utilities;
#if NET35
using Iesi.Collections.Generic;
#endif

namespace Cassette
{
    class BundleContainer : IBundleContainer
    {
        public BundleContainer(IEnumerable<Bundle> bundles)
        {
            this.bundles = bundles.ToArray(); // Force eval to prevent repeatedly generating new bundles.

            ValidateBundleReferences();
            ValidateAssetReferences();
            bundleImmediateReferences = BuildBundleImmediateReferenceDictionary();
        }

        readonly Bundle[] bundles;
        readonly Dictionary<Bundle, HashedSet<Bundle>> bundleImmediateReferences;

        public IEnumerable<Bundle> Bundles
        {
            get { return bundles; }
        }

        public IEnumerable<Bundle> IncludeReferencesAndSortBundles(IEnumerable<Bundle> bundlesToSort)
        {
            var bundlesArray = bundlesToSort.ToArray();
            var references = GetBundleReferencesWithImplicitOrderingIncluded(bundlesArray);
            var all = new HashedSet<Bundle>();
            foreach (var bundle in bundlesArray)
            {
                AddBundlesReferencedBy(bundle, all);   
            }
            var graph = BuildBundleGraph(references, all);
            var cycles = graph.FindCycles().ToArray();
            if (cycles.Length > 0)
            {
                var details = string.Join(Environment.NewLine, cycles.Select(cycle => "[" + string.Join(", ", cycle.Select(m => m.Path).ToArray()) + "]").ToArray());
                throw new InvalidOperationException(
                    "Cycles detected in bundle dependency graph:" + Environment.NewLine +
                    details
                );
            }
            return graph.TopologicalSort();
        }

        Graph<Bundle> BuildBundleGraph(IDictionary<Bundle, HashedSet<Bundle>> references, IEnumerable<Bundle> all)
        {
            return new Graph<Bundle>(
                all,
                bundle =>
                {
                    HashedSet<Bundle> set;
                    if (references.TryGetValue(bundle, out set)) return set;
                    return Enumerable.Empty<Bundle>();
                }
            );
        }

        Dictionary<Bundle, HashedSet<Bundle>> GetBundleReferencesWithImplicitOrderingIncluded(IEnumerable<Bundle> initialBundles)
        {
            var roots = initialBundles.Where(m =>
            {
                HashedSet<Bundle> set;
                if (bundleImmediateReferences.TryGetValue(m, out set)) return set.Count == 0;
                return true;
            }).ToList();

            // Clone the original references dictionary, so we can add the extra
            // implicit references based on array order.
            var references = new Dictionary<Bundle, HashedSet<Bundle>>();
            foreach (var reference in bundleImmediateReferences)
            {
                references[reference.Key] = new HashedSet<Bundle>(reference.Value);
            }
            for (var i = 1; i < roots.Count; i++)
            {
                var bundle = roots[i];
                var previous = roots[i - 1];
                HashedSet<Bundle> set;
                if (!references.TryGetValue(bundle, out set))
                {
                    references[bundle] = set = new HashedSet<Bundle>();
                }
                set.Add(previous);
            }
            return references;
        }

        void AddBundlesReferencedBy(Bundle bundle, ISet<Bundle> all)
        {
            if (all.Contains(bundle)) return;
            all.Add(bundle);

            HashedSet<Bundle> referencedBundles;
            if (!bundleImmediateReferences.TryGetValue(bundle, out referencedBundles)) return;
            foreach (var referencedBundle in referencedBundles)
            {
                AddBundlesReferencedBy(referencedBundle, all);
            }
        }

        public IEnumerable<Bundle> FindBundlesContainingPath(string path)
        {
            path = PathUtilities.AppRelative(path);
            return bundles.Where(bundle => bundle.ContainsPath(path));
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
            var message = string.Join(Environment.NewLine, notFound.ToArray());
            if (message.Length > 0)
            {
                throw new AssetReferenceException(message);
            }
        }

        void ValidateAssetReferences()
        {
            var collector = new BundleReferenceCollector(AssetReferenceType.DifferentBundle);
            foreach (var bundle in Bundles)
            {
                bundle.Accept(collector);
            }
            var notFound = from reference in collector.CollectedReferences
                           where !reference.SourceBundle.IsFromDescriptorFile
                              && NoBundlesContainPath(reference.AssetReference.Path)
                           select CreateAssetReferenceNotFoundMessage(reference.AssetReference);

            var message = string.Join(Environment.NewLine, notFound.ToArray());
            if (message.Length > 0)
            {
                throw new AssetReferenceException(message);
            }
        }

        bool NoBundlesContainPath(string path)
        {
            return !bundles.Any(m => m.ContainsPath(path));
        }

        Dictionary<Bundle, HashedSet<Bundle>> BuildBundleImmediateReferenceDictionary()
        {
            return (
                from bundle in bundles
                select new 
                { 
                    bundle,
                    references = new HashedSet<Bundle>(GetNonSameBundleAssetReferences(bundle)
                        .Select(r => r.Path)
                        .Concat(bundle.References)
                        .SelectMany(FindBundlesContainingPath).ToList()
                    ) 
                }
            ).ToDictionary(x => x.bundle, x => x.references);
        }

        IEnumerable<AssetReference> GetNonSameBundleAssetReferences(Bundle bundle)
        {
            var collector = new BundleReferenceCollector(AssetReferenceType.DifferentBundle, AssetReferenceType.Url);
            bundle.Accept(collector);
            return collector.CollectedReferences.Select(r => r.AssetReference);
        }

        string CreateAssetReferenceNotFoundMessage(AssetReference reference)
        {
            if (reference.SourceLineNumber > 0)
            {
                return string.Format(
                    "Reference error in \"{0}\", line {1}. Cannot find \"{2}\".",
                    reference.SourceAsset.Path, reference.SourceLineNumber, reference.Path
                );
            }
            else
            {
                return string.Format(
                    "Reference error in \"{0}\". Cannot find \"{1}\".",
                    reference.SourceAsset.Path, reference.Path
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