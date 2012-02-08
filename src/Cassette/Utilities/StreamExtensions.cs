using System.IO;
using System.Security.Cryptography;

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

        public static long CopyTo(this Stream source, Stream target)
        {
            const int bufSize = 0x1000;

            byte[] buf = new byte[bufSize];

            long totalBytes = 0;

            int bytesRead = 0;

            while ((bytesRead = source.Read(buf, 0, bufSize)) > 0)
            {

                target.Write(buf, 0, bytesRead);

                totalBytes += bytesRead;

            }

            return totalBytes;
        }
    }
}

