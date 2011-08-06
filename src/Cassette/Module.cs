using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace Cassette
{
    public class Module : IDisposable
    {
        public Module(string directory)
        {
            this.directory = NormalizePath(directory);
        }

        readonly string directory;
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

        public bool ContainsPath(string path)
        {
            if (IsModulePath(path)) return true;
            return assets.Any(a => a.SourceFilename.Equals(path, StringComparison.OrdinalIgnoreCase));
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

        public void Dispose()
        {
            foreach (var asset in assets.OfType<IDisposable>())
            {
                asset.Dispose();
            }
        }
    }
}
