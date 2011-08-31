using System.IO;

namespace Cassette
{
    public interface IFile
    {
        /// <summary>
        /// The directory containing the file.
        /// </summary>
        IFileSystem Directory { get; }

        /// <summary>
        /// Opens a stream to the file.
        /// </summary>
        Stream Open(FileMode mode, FileAccess access);
    }
}