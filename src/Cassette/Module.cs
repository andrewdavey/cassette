using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Web;

namespace Cassette
{
    public class Module : IDisposable
    {
        public Module(string relativeDirectory)
        {
            this.directory = NormalizePath(relativeDirectory);
        }

        readonly string directory;
        IList<IAsset> assets = new List<IAsset>();
        HashSet<Module> references = new HashSet<Module>();
        HashSet<IAsset> compiledAssets = new HashSet<IAsset>();

        public string Directory
        {
            get { return directory; }
        }

        public IList<IAsset> Assets
        {
            get { return assets; }
            set { assets = value; }
        }

        public string ContentType { get; set; }
        public string Location { get; set; }

        public virtual void Process(ICassetteApplication application)
        {
        }

        public bool ContainsPath(string path)
        {
            return new ModuleContainsPathPredicate().ModuleContainsPath(path, this);
        }

        public IAsset FindAssetByPath(string path)
        {
            var slashes = new char[] { Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar };
            var pathSegments = path.Split(slashes);
            return Assets.FirstOrDefault(
                a => a.SourceFilename.Split(slashes)
                                     .SequenceEqual(pathSegments, StringComparer.OrdinalIgnoreCase)
            );
        }

        bool IsModulePath(string path)
        {
            return directory.Equals(
                NormalizePath(path),
                StringComparison.OrdinalIgnoreCase
            );
        }

        string NormalizePath(string path)
        {
            return path.TrimEnd(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar);
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
