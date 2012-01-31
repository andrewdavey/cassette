﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Cassette.BundleProcessing;
using Cassette.Configuration;
using Cassette.Utilities;

namespace Cassette
{
    [System.Diagnostics.DebuggerDisplay("{Path}")]
    public abstract class Bundle : IDisposable
    {
        readonly string path;
        readonly List<IAsset> assets = new List<IAsset>();
        readonly HashSet<string> references = new HashSet<string>();
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

        internal string PathWithoutPrefix
        {
            get { return path.TrimStart('~', '/'); }
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
        public byte[] Hash
        {
            get
            {
                if (assets.Count == 0)
                {
                    return new byte[0];
                }
                if (assets.Count > 1)
                {
                    return new ConcatenatedAsset(assets).Hash;
                }
                return assets[0].Hash;
            }
        }

        internal IEnumerable<string> References
        {
            get { return references; }
        }

        // When bundle loaded from cache we don't need to do most of the asset processing.
        // However some steps, like assigning the renderer still need to happen.
        internal bool IsFromCache { get; set; }

        internal bool IsSorted { get; set; }

        /// <summary>
        /// Opens a readable stream of the contained assets content.
        /// </summary>
        /// <returns>A readable stream.</returns>
        public Stream OpenStream()
        {
            RequireSingleAsset();
            return Assets[0].OpenStream();
        }

        /// <summary>
        /// Adds a reference to another bundle.
        /// </summary>
        /// <param name="bundlePathOrUrl">The application relative path of the bundle, or a external URL.</param>
        public void AddReference(string bundlePathOrUrl)
        {
            references.Add(ConvertReferenceToAppRelative(bundlePathOrUrl));
        }

        void RequireSingleAsset()
        {
            if (assets.Count == 0)
            {
                throw new InvalidOperationException(
                    "Invalid operation when bundle has no assets."
                );
            }
            if (assets.Count > 1)
            {
                throw new InvalidOperationException(
                    "Invalid operation when bundle assets are not concatenated into a single asset."
                );
            }
        }

        internal abstract void Process(CassetteSettings settings);

        internal abstract string Render();

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

        internal void Accept(IBundleVisitor visitor)
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
                        cycle => "[" + string.Join(", ", cycle.Select(a => a.SourceFile.FullPath)) + "]"
                    )
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