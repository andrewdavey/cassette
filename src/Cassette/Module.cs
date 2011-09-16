using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Cassette.ModuleProcessing;
using Cassette.Utilities;

namespace Cassette
{
    [System.Diagnostics.DebuggerDisplay("{Path}")]
    public class Module : IDisposable
    {
        public Module(string applicationRelativePath)
        {
            if (applicationRelativePath.IsUrl())
            {
                path = applicationRelativePath;
            }
            else
            {
                if (applicationRelativePath.StartsWith("~") == false)
                {
                    throw new ArgumentException("Module path must be application relative (starting with a '~').");
                }
                path = PathUtilities.NormalizePath(applicationRelativePath);
            }
        }

        protected Module()
        {
            // Protected constructor to allow InlineScriptModule to be created without a path.
        }

        readonly string path;
        IList<IAsset> assets = new List<IAsset>();
        bool hasSortedAssets;
        readonly HashSet<string> references = new HashSet<string>();

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

        public byte[] Hash
        {
            get
            {
                if (Assets.Count == 1)
                {
                    return Assets[0].Hash;
                }
                else
                {
                    throw new InvalidOperationException("Module Hash is only available when the module contains a single asset.");
                }
            }
        }

        public IEnumerable<string> References
        {
            get { return references; }
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

        public void AddReferences(IEnumerable<string> references)
        {
            foreach (var reference in references)
            {
                this.references.Add(ConvertReferenceToAppRelative(reference));
            }
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

        public virtual bool ContainsPath(string path)
        {
            return new ModuleContainsPathPredicate().ModuleContainsPath(path, this);
        }

        public IAsset FindAssetByPath(string path)
        {
            return Assets.FirstOrDefault(
                a => PathUtilities.PathsEqual(a.SourceFilename, path)
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

        public void SortAssetsByDependency()
        {
            if (hasSortedAssets) return;
            // Graph topological sort, based on references between assets.
            var assetsByFilename = Assets.ToDictionary(
                a => a.SourceFilename,
                StringComparer.OrdinalIgnoreCase
            );
            var graph = new Graph<IAsset>(
                Assets,
                asset => asset.References
                    .Where(reference => reference.Type == AssetReferenceType.SameModule)
                    .Select(reference => assetsByFilename[reference.Path])
            );
            var cycles = graph.FindCycles().ToArray();
            if (cycles.Length > 0)
            {
                var details = string.Join(
                    Environment.NewLine,
                    cycles.Select(
                        cycle => "[" + string.Join(", ", cycle.Select(a => a.SourceFilename)) + "]"
                    )
                );
                throw new InvalidOperationException("Cycles detected in asset references:" + Environment.NewLine + details);
            }
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

        internal bool PathIsPrefixOf(string path)
        {
            if (path.Length < Path.Length) return false;
            var prefix = path.Substring(0, Path.Length);
            return PathUtilities.PathsEqual(prefix, Path);
        }
    }
}