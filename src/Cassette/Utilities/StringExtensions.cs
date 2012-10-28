using System;

namespace Cassette.Utilities
{
    /// <summary>
    /// Utility methods for strings.
    /// </summary>
    public static class StringExtensions
    {
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