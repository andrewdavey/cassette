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

        /// <summary>
        /// Gets if the file exists.
        /// </summary>
        bool Exists { get; }

        /// <summary>
        /// Gets the last write time (UTC) of the file.
        /// </summary>
        DateTime LastWriteTimeUtc { get; }

        /// <summary>
        /// Gets the full application relative path of the file.
        /// </summary>
        string FullPath { get; }

        /// <summary>
        /// Gets the full path to the file
        /// </summary>
        /// <returns></returns>
        string FullSystemPath { get; }
     
        /// <summary>
        /// Opens a stream to the file.
        /// </summary>
        Stream Open(FileMode mode, FileAccess access, FileShare fileShare);

        /// <summary>
        /// Deletes the file.
        /// </summary>
        void Delete();
    }
}