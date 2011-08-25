using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Cassette.Utilities;

namespace Cassette
{
    public class Module : IDisposable
    {
        public Module(string relativeDirectory)
        {
            path = NormalizePath(relativeDirectory);
        }

        readonly string path;
        IList<IAsset> assets = new List<IAsset>();
        readonly HashSet<IAsset> compiledAssets = new HashSet<IAsset>();
        static readonly char[] Slashes = new[] { System.IO.Path.DirectorySeparatorChar, System.IO.Path.AltDirectorySeparatorChar };

        public string Path
        {
            get { return path; }
        }

        public IList<IAsset> Assets
        {
            get { return assets; }
            set { assets = value; }
        }

        public void AddAssets(IEnumerable<IAsset> newAssets, bool preSorted)
        {
            foreach (var asset in newAssets)
            {
                assets.Add(asset);
            }
            HasSortedAssets = preSorted;
        }

        bool HasSortedAssets { get; set; }

        public string ContentType { get; set; }
        public string Location { get; set; }

        public virtual bool IsPersistent
        {
            get { return true; }
        }

        public virtual void Process(ICassetteApplication application) { }

        public bool ContainsPath(string relativePath)
        {
            return new ModuleContainsPathPredicate().ModuleContainsPath(relativePath, this);
        }

        public IAsset FindAssetByPath(string relativePath)
        {
            var pathSegments = relativePath.Split(Slashes);
            return Assets.FirstOrDefault(
                a => a.SourceFilename
                      .Split(Slashes)
                      .SequenceEqual(pathSegments, StringComparer.OrdinalIgnoreCase)
            );
        }

        string NormalizePath(string relativePath)
        {
            return relativePath.TrimEnd(Slashes);
        }

        public void Accept(IAssetVisitor visitor)
        {
            visitor.Visit(this);
            foreach (var asset in assets)
            {
                asset.Accept(visitor);
            }
        }

        public virtual IHtmlString Render(ICassetteApplication application)
        {
            return new HtmlString("");
        }

        public void RegisterCompiledAsset(IAsset asset)
        {
            compiledAssets.Add(asset);
        }

        protected bool IsCompiledAsset(IAsset asset)
        {
            return compiledAssets.Contains(asset);
        }

        public void SortAssetsByDependency()
        {
            if (HasSortedAssets) return;
            // Graph topological sort, based on references between assets.
            var assetsByFilename = Assets.ToDictionary(
                a => System.IO.Path.Combine(Path, a.SourceFilename),
                StringComparer.OrdinalIgnoreCase
            );
            var graph = new Graph<IAsset>(
                Assets,
                asset => asset.References
                    .Where(reference => reference.Type == AssetReferenceType.SameModule)
                    .Select(reference => assetsByFilename[reference.ReferencedPath])
            );
            assets = graph.TopologicalSort().ToList();
            HasSortedAssets = true;
        }

        public void Dispose()
        {
            foreach (var asset in assets.OfType<IDisposable>())
            {
                asset.Dispose();
            }
        }
    }
}
