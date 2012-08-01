using System.IO;
using System.Security.Cryptography;

#if NET35
using System.IO.IsolatedStorage;
#endif

namespace Cassette.Utilities
{
    static class StreamExtensions
    {
// ReSharper disable InconsistentNaming
        public static byte[] ComputeSHA1Hash(this Stream stream)
// ReSharper restore InconsistentNaming
        {
            using (var sha1 = SHA1.Create())
            {
                return sha1.ComputeHash(stream);
            }
        }

        public static string ReadToEnd(this Stream stream)
        {
            using (var reader = new StreamReader(stream))
            {
                return reader.ReadToEnd();
            }
        }

#if NET35
// ReSharper disable UnusedMethodReturnValue.Global
        public static long CopyTo(this Stream source, Stream target)
// ReSharper restore UnusedMethodReturnValue.Global
        {
            const int bufSize = 0x1000;
            var buf = new byte[bufSize];
            long totalBytes = 0;
            int bytesRead;

            while ((bytesRead = source.Read(buf, 0, bufSize)) > 0)
            {
                target.Write(buf, 0, bytesRead);
                totalBytes += bytesRead;
            }

            return totalBytes;
        }

        public static IsolatedStorageFileStream CreateFile(this IsolatedStorageFile storage, string path)
        {
            return new IsolatedStorageFileStream(path, FileMode.Create, FileAccess.ReadWrite, FileShare.None, storage);
        }

        public static IsolatedStorageFileStream OpenFile(this IsolatedStorageFile storage, string path, FileMode mode, FileAccess access)
        {
            return new IsolatedStorageFileStream(path, mode, access, storage);
        }

        public static bool FileExists(this IsolatedStorageFile storage, string fileName)
        {
            return storage.GetFileNames(fileName).Length > 0;
        }
#endif

    }
}

