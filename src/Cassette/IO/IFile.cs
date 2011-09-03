using System;
using System.IO;

namespace Cassette.IO
{
    public interface IFile
    {
        /// <summary>
        /// The directory containing the file.
        /// </summary>
        IDirectory Directory { get; }

        bool Exists { get; }

        DateTime LastWriteTimeUtc { get; }

        string FullPath { get; }
     
        /// <summary>
        /// Opens a stream to the file.
        /// </summary>
        Stream Open(FileMode mode, FileAccess access);
    }
}