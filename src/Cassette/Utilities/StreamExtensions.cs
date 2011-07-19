using System.IO;
using System.Security.Cryptography;

namespace Cassette.Utilities
{
    public static class StreamExtensions
    {
        public static byte[] ComputeSHA1Hash(this Stream stream)
        {
            using (var sha1 = SHA1.Create())
            {
                return sha1.ComputeHash(stream);
            }
        }
    }
}
