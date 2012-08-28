#if !NET35
using System;
using System.IO;
using Cassette.IO;
using Microsoft.Scripting;

namespace Cassette.Stylesheets
{
    /// <summary>
    /// File IO for relative paths is directed to an <see cref="IDirectory"/> implementation.
    /// All other paths are passed to the inner <see cref="PlatformAdaptationLayer"/>.
    /// </summary>
    class CassettePlatformAdaptationLayer : PlatformAdaptationLayer
    {
        readonly PlatformAdaptationLayer innerPal;
        readonly Func<IDirectory> getDirectory;

        public CassettePlatformAdaptationLayer(PlatformAdaptationLayer innerPal, Func<IDirectory> getDirectory)
        {
            this.innerPal = innerPal;
            this.getDirectory = getDirectory;
        }

        public Action<string> OnOpenInputFileStream = delegate {};

        public override bool FileExists(string path)
        {
            if (IsRelativePath(path))
            {
                path = RemoveDotSlash(path);
                return getDirectory().GetFile(path).Exists;
            }
            else
            {
                return innerPal.FileExists(path);
            }
        }

        public override Stream OpenInputFileStream(string path)
        {
            if (IsRelativePath(path))
            {
                path = RemoveDotSlash(path);
                var file = getDirectory().GetFile(path);
                if (OnOpenInputFileStream != null) OnOpenInputFileStream(file.FullPath);
                return file.OpenRead();
            }
            else
            {
                return innerPal.OpenInputFileStream(path);
            }
        }

        public override Stream OpenInputFileStream(string path, FileMode mode, FileAccess access, FileShare share, int bufferSize)
        {
            if (IsRelativePath(path))
            {
                path = RemoveDotSlash(path);
                var file = getDirectory().GetFile(path);
                if (OnOpenInputFileStream != null) OnOpenInputFileStream(file.FullPath);
                return file.Open(mode, access, share);
            }
            else
            {
                return innerPal.OpenInputFileStream(path, mode, access, share, bufferSize);
            }
        }

        public override Stream OpenInputFileStream(string path, FileMode mode, FileAccess access, FileShare share)
        {
            if (IsRelativePath(path))
            {
                path = RemoveDotSlash(path);
                var file = getDirectory().GetFile(path);
                if (OnOpenInputFileStream != null) OnOpenInputFileStream(file.FullPath);
                return file.Open(mode, access, share);
            }
            else
            {
                return innerPal.OpenInputFileStream(path, mode, access, share);
            }
        }

        static string RemoveDotSlash(string path)
        {
            if (path.StartsWith("./")) path = path.Substring(2);
            return path;
        }

        static bool IsRelativePath(string path)
        {
            var isRooted = path.Length > 2 && char.IsLetter(path[0]) && path[1] == ':';
            return !isRooted;
        }
    }
}
#endif