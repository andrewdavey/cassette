using System;
using System.Linq;

namespace Knapsack.Utilities
{
    public static class ByteArrayExtensions
    {
        public static string ToHexString(this byte[] bytes)
        {
            return string.Concat(bytes.Select(b => b.ToString("x2")));
        }

        public static byte[] FromHexString(string hex)
        {
            var bytes = new byte[hex.Length / 2];
            for (int i = 0; i < hex.Length; i += 2)
            {
                bytes[i / 2] = Convert.ToByte(hex.Substring(i, 2), 16);
            }
            return bytes;
        }
    }
}
