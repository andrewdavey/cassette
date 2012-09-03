using System.IO;

namespace Cassette.IO
{
    public static class FileExtensions
    {
        public static Stream OpenRead(this IFile file)
        {
            return file.Open(FileMode.Open, FileAccess.Read, FileShare.ReadWrite);
        }
    }
}
