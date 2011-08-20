using System.IO;
using System.Security.Cryptography;

namespace Cassette.Utilities
{
    public static class StreamExtensions
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
    }
}
