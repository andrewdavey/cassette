using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Xml.Linq;
using Cassette.ModuleProcessing;
using Cassette.Utilities;
using Cassette.Persistence;

namespace Cassette
{
    public class Module : IDisposable
    {
        public Module(string relativeDirectory)
        {
            if (relativeDirectory.IsUrl())
            {
                path = relativeDirectory;
            }
            else
            {
                path = PathUtilities.NormalizePath(relativeDirectory);
            }
        }

        readonly string path;
        IList<IAsset> assets = new List<IAsset>();
        bool hasSortedAssets;
        readonly HashSet<IAsset> compiledAssets = new HashSet<IAsset>();
        static readonly char[] Slashes = new[] { System.IO.Path.DirectorySeparatorChar, System.IO.Path.AltDirectorySeparatorChar };
        readonly List<string> references = new List<string>();

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
            this.references.AddRange(references.Select(ConvertReferenceToAppRelative));
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
                    "~",
                    Path,
                    reference
                ));
            }
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
                a => PathUtilities.CombineWithForwardSlashes("~", Path, a.SourceFilename),
                StringComparer.OrdinalIgnoreCase
            );
            var graph = new Graph<IAsset>(
                Assets,
                asset => asset.References
                    .Where(reference => reference.Type == AssetReferenceType.SameModule)
                    .Select(reference => assetsByFilename[reference.Path])
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

        public virtual IEnumerable<XElement> CreateCacheManifest()
        {
            var element = new XElement("Module",
                new XAttribute("Path", Path)
            );
            if (Assets.Count == 1)
            {
                element.Add(new XAttribute("Hash", Hash.ToHexString()));
            }
            if (string.IsNullOrEmpty(Location) == false)
            {
                element.Add(new XAttribute("Location", Location));
            }
            if (string.IsNullOrEmpty(ContentType) == false)
            {
                element.Add(new XAttribute("ContentType", ContentType));
            }
            var asset = assets.OfType<ICacheableAsset>().SingleOrDefault();
            if (asset!= null)
            {
                element.Add(asset.CreateCacheManifest());
            }
            foreach (var reference in references)
            {
                element.Add(new XElement("Reference", new XAttribute("Path", reference)));
            }
            yield return element;
        }

        public virtual void InitializeFromManifest(XElement element)
        {       
        }
    }
}