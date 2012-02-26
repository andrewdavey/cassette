using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Cassette.BundleProcessing;
using Cassette.Configuration;
using Cassette.Manifests;
using Cassette.Utilities;
#if NET35
using Iesi.Collections.Generic;
#endif

namespace Cassette
{
    [System.Diagnostics.DebuggerDisplay("{Path}")]
    public abstract class Bundle : IDisposable
    {
        readonly string path;
        readonly List<IAsset> assets = new List<IAsset>();
        readonly HashedSet<string> references = new HashedSet<string>();
        readonly HtmlAttributeDictionary htmlAttributes = new HtmlAttributeDictionary();

        protected Bundle(string applicationRelativePath)
        {
            if (applicationRelativePath == null) throw new ArgumentNullException("applicationRelativePath");
            path = PathUtilities.AppRelative(applicationRelativePath);
        }

        protected Bundle()
        {
            // Protected constructor to allow InlineScriptBundle to be created without a path.
        }

        /// <summary>
        /// The application relative path of the bundle.
        /// </summary>
        public string Path
        {
            get { return path; }
        }

        /// <summary>
        /// The value sent in the HTTP Content-Type header.
        /// </summary>
        public string ContentType { get; set; }

        /// <summary>
        /// Defines where to render this bundle in an HTML page.
        /// </summary>
        public string PageLocation { get; set; }

        /// <summary>
        /// Attributes to include with the rendered element.
        /// </summary>
        public HtmlAttributeDictionary HtmlAttributes
        {
            get { return htmlAttributes; }
        }

        /// <summary>
        /// The assets contained in the bundle.
        /// </summary>
        public IList<IAsset> Assets
        {
            get { return assets; }
        }

        /// <summary>
        /// Gets the hash of the combined assets.
        /// </summary>
        public byte[] Hash { get; internal set; }

        internal virtual string Url
        {
            get
            {
                var type = GetType().Name.ToLowerInvariant();
                var pathWithoutPrefix = path.TrimStart('~', '/');
                return type + "/" + pathWithoutPrefix + "_" + Hash.ToHexString();
            }
        }

        internal IEnumerable<string> References
        {
            get { return references; }
        }

        internal bool IsSorted { get; set; }

        /// <summary>
        /// Opens a readable stream of the contained assets content.
        /// </summary>
        /// <returns>A readable stream.</returns>
        public Stream OpenStream()
        {
            if (assets.Count == 0) return Stream.Null;
            return assets[0].OpenStream();
        }

        /// <summary>
        /// Adds a reference to another bundle.
        /// </summary>
        /// <param name="bundlePathOrUrl">The application relative path of the bundle, or a external URL.</param>
        public void AddReference(string bundlePathOrUrl)
        {
            references.Add(ConvertReferenceToAppRelative(bundlePathOrUrl));
        }

        internal void Process(CassetteSettings settings)
        {
            if (IsProcessed)
            {
                throw new InvalidOperationException("Bundle has already been processed.");
            }
            ProcessCore(settings);
            IsProcessed = true;
        }

        protected abstract void ProcessCore(CassetteSettings settings);

        internal bool IsProcessed { get; private set; }

        internal abstract string Render();

        internal BundleManifest CreateBundleManifest()
        {
            return CreateBundleManifest(IsProcessed);
        }

        internal abstract BundleManifest CreateBundleManifest(bool includeProcessedBundleContent);

        internal virtual bool ContainsPath(string pathToFind)
        {
            return new BundleContainsPathPredicate().BundleContainsPath(pathToFind, this);
        }

        internal IAsset FindAssetByPath(string pathToFind)
        {
            var assetFinder = new AssetFinder(pathToFind);
            Accept(assetFinder);
            return assetFinder.FoundAsset;
        }

        public void Accept(IBundleVisitor visitor)
        {
            visitor.Visit(this);
            foreach (var asset in assets)
            {
                asset.Accept(visitor);
            }
        }

        internal void SortAssetsByDependency()
        {
            if (IsSorted) return;
            // Graph topological sort, based on references between assets.
            var assetsByFilename = Assets.ToDictionary(
                a => a.SourceFile.FullPath,
                StringComparer.OrdinalIgnoreCase
            );
            var graph = new Graph<IAsset>(
                Assets,
                asset => asset.References
                    .Where(reference => reference.Type == AssetReferenceType.SameBundle)
                    .Select(reference => assetsByFilename[reference.Path])
            );
            var cycles = graph.FindCycles().ToArray();
            if (cycles.Length > 0)
            {
                var details = string.Join(
                    Environment.NewLine,
                    cycles.Select(
                        cycle => "[" + string.Join(", ", cycle.Select(a => a.SourceFile.FullPath).ToArray()) + "]"
                    ).ToArray()
                );
                throw new InvalidOperationException("Cycles detected in asset references:" + Environment.NewLine + details);
            }
            assets.Clear();
            assets.AddRange(graph.TopologicalSort());
            IsSorted = true;
        }

        internal void ConcatenateAssets()
        {
            if (assets.Count == 0) return;

            Trace.Source.TraceInformation("Concatenating assets of {0}", path);
            var concatenated = new ConcatenatedAsset(assets);
            assets.Clear();
            assets.Add(concatenated);
            Trace.Source.TraceInformation("Concatenated assets of {0}", path);
        }

        string ConvertReferenceToAppRelative(string reference)
        {
            if (reference.IsUrl()) return reference;

            if (reference.StartsWith("~"))
            {
                return PathUtilities.NormalizePath(reference);
            }
            else if (reference.StartsWith("/"))
            {
                return PathUtilities.NormalizePath("~" + reference);
            }
            else
            {
                return PathUtilities.NormalizePath(PathUtilities.CombineWithForwardSlashes(
                    Path,
                    reference
                ));
            }
        }

        protected virtual void Dispose(bool disposing)
        {
            if (!disposing) return;
            foreach (var asset in assets.OfType<IDisposable>())
            {
                asset.Dispose();
            }
        }

        void IDisposable.Dispose()
        {
            Dispose(true);
        }
    }
}