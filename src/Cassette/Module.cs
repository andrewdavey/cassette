using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

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
        static readonly char[] slashes = new[] { System.IO.Path.DirectorySeparatorChar, System.IO.Path.AltDirectorySeparatorChar };

        public string Path
        {
            get { return path; }
        }

        public IList<IAsset> Assets
        {
            get { return assets; }
            set { assets = value; }
        }

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
            var pathSegments = relativePath.Split(slashes);
            return Assets.FirstOrDefault(
                a => a.SourceFilename
                      .Split(slashes)
                      .SequenceEqual(pathSegments, StringComparer.OrdinalIgnoreCase)
            );
        }

        string NormalizePath(string relativePath)
        {
            return relativePath.TrimEnd(slashes);
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

        public void Dispose()
        {
            foreach (var asset in assets.OfType<IDisposable>())
            {
                asset.Dispose();
            }
        }
    }
}
