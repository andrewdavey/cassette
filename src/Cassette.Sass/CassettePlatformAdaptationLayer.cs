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
                return getDirectory().GetFile(path.Substring(2)).Exists;
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
                if (OnOpenInputFileStream != null) OnOpenInputFileStream(path);
                return getDirectory().GetFile(path.Substring(2)).OpenRead();
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
                if (OnOpenInputFileStream != null) OnOpenInputFileStream(path);
                return getDirectory().GetFile(path.Substring(2)).Open(mode, access, share);
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
                if (OnOpenInputFileStream != null) OnOpenInputFileStream(path);
                return getDirectory().GetFile(path.Substring(2)).Open(mode, access, share);
            }
            else
            {
                return innerPal.OpenInputFileStream(path, mode, access, share);
            }
        }

        static bool IsRelativePath(string path)
        {
            return path.StartsWith("./");
        }
    }
}
#endif