using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace Cassette
{
    public class Module : IDisposable
    {
        public Module(string relativeDirectory, Func<string, string> getFullPath)
        {
            this.directory = NormalizePath(relativeDirectory);
            this.getFullPath = getFullPath;
        }

        readonly string directory;
        readonly Func<string, string> getFullPath;
        IList<IAsset> assets = new List<IAsset>();

        public string Directory
        {
            get { return directory; }
        }

        public IList<IAsset> Assets
        {
            get { return assets; }
            set { assets = value; }
        }

        public string GetFullPath(string path)
        {
            return getFullPath(Path.Combine(directory, path));
        }

        public bool ContainsPath(string path)
        {
            return new AssetFinder().Contains(path, this);
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

        public void Dispose()
        {
            foreach (var asset in assets.OfType<IDisposable>())
            {
                asset.Dispose();
            }
        }
    }
}
