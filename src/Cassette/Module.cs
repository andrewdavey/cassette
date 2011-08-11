using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Web;

namespace Cassette
{
    public class Module : IDisposable
    {
        public Module(string relativeDirectory, IFileSystem fileSystem)
        {
            this.directory = NormalizePath(relativeDirectory);
            this.fileSystem = fileSystem.NavigateTo(relativeDirectory, false);
        }

        readonly string directory;
        readonly IFileSystem fileSystem;
        IList<IAsset> assets = new List<IAsset>();
        HashSet<Module> references = new HashSet<Module>();

        public IFileSystem FileSystem
        {
            get { return fileSystem; }
        }

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

        public bool ContainsPath(string path)
        {
            return new ModuleContainsPathPredicate().ModuleContainsPath(path, this);
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

        public void Dispose()
        {
            foreach (var asset in assets.OfType<IDisposable>())
            {
                asset.Dispose();
            }
        }
    }
}
