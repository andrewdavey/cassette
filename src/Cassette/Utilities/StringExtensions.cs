using System;
using System.IO;

namespace Cassette.Utilities
{
    /// <summary>
    /// Utility methods for strings.
    /// </summary>
    static class StringExtensions
    {
        /// <summary>
        /// Returns a new stream containing the contents of the string, using UTF-8 encoding.
        /// The stream's Position property is set to zero.
        /// </summary>
        /// <param name="s">The string to convert into a stream.</param>
        /// <returns>A new stream.</returns>
        public static Stream AsStream(this string s)
        {
            var source = new MemoryStream();
            var writer = new StreamWriter(source);
            writer.Write(s);
            writer.Flush();
            source.Position = 0;
            return source;
        }

        public static bool IsUrl(this string s)
        {
            return s.StartsWith("http:", StringComparison.OrdinalIgnoreCase)
                || s.StartsWith("https:", StringComparison.OrdinalIgnoreCase)
                || s.StartsWith("//");
        }

        public static bool IsNullOrWhiteSpace(this string s)
        {
#if NET35
            return String.IsNullOrEmpty(s) || s.Trim().Length == 0;
#else
            return String.IsNullOrWhiteSpace(s);
#endif
        }
    }
}