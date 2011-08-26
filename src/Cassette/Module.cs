using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Cassette.ModuleProcessing;
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
        bool hasSortedAssets;
        readonly HashSet<IAsset> compiledAssets = new HashSet<IAsset>();
        static readonly char[] Slashes = new[] { System.IO.Path.DirectorySeparatorChar, System.IO.Path.AltDirectorySeparatorChar };

        public string Path
        {
            get { return path; }
        }

        public IList<IAsset> Assets
        {
            get { return assets; }
        }

        public string ContentType { get; set; }

        public string Location { get; set; }

        public virtual bool IsPersistent
        {
            get { return true; }
        }

        public virtual void Process(ICassetteApplication application)
        {
        }

        public void AddAssets(IEnumerable<IAsset> newAssets, bool preSorted)
        {
            foreach (var asset in newAssets)
            {
                assets.Add(asset);
            }
            hasSortedAssets = preSorted;
        }

        public bool ContainsPath(string path)
        {
            return new ModuleContainsPathPredicate().ModuleContainsPath(path, this);
        }

        public IAsset FindAssetByPath(string path)
        {
            if (path.StartsWith("~"))
            {
                // Asset path must be relative to the Module. So remove the module's path from the start.
                if (Path.Length > 0)
                {
                    path = path.Substring(Path.Length + 3); // +3 to also remove the "~/ModulePath/" prefix.
                }
                else
                {
                    path = path.Substring(2); // +2 to remove the "~/" prefix.
                }
            }
            var pathSegments = path.Split(Slashes);
            return Assets.FirstOrDefault(
                a => a.SourceFilename
                      .Split(Slashes)
                      .SequenceEqual(pathSegments, StringComparer.OrdinalIgnoreCase)
            );
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
            if (hasSortedAssets) return;
            // Graph topological sort, based on references between assets.
            var assetsByFilename = Assets.ToDictionary(
                a => System.IO.Path.Combine("~", Path, a.SourceFilename),
                StringComparer.OrdinalIgnoreCase
            );
            var graph = new Graph<IAsset>(
                Assets,
                asset => asset.References
                    .Where(reference => reference.Type == AssetReferenceType.SameModule)
                    .Select(reference => assetsByFilename[reference.ReferencedPath])
            );
            assets = graph.TopologicalSort().ToList();
            hasSortedAssets = true;
        }

        public void ConcatenateAssets()
        {
            assets = new List<IAsset>(new[]
            { 
                new ConcatenatedAsset(assets)
            });
        }

        public void Dispose()
        {
            foreach (var asset in assets.OfType<IDisposable>())
            {
                asset.Dispose();
            }
        }

        string NormalizePath(string relativePath)
        {
            return relativePath.TrimEnd(Slashes);
        }
    }
}
