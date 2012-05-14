using System;
using System.Collections.Generic;
using System.Linq;

namespace Cassette.Utilities
{
    static class ByteArrayExtensions
    {
        public static string ToHexString(this IEnumerable<byte> bytes)
        {
            return string.Concat(bytes.Select(b => b.ToString("x2")).ToArray());
        }

        public static byte[] FromHexString(string hex)
        {
            var bytes = new byte[hex.Length / 2];
            for (var i = 0; i < hex.Length; i += 2)
            {
                bytes[i / 2] = Convert.ToByte(hex.Substring(i, 2), 16);
            }
            return bytes;
        }

        public static string ToUrlSafeBase64String(this byte[] bytes)
        {
            return Convert
                .ToBase64String(bytes)
                .Replace('+', '-')
                .Replace('/', '_');
        }
    }
}