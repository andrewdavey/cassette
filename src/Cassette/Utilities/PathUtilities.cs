using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace Cassette.Utilities
{
    public static class PathUtilities
    {
        public static string Combine(params string[] paths)
        {
#if !NET35
            return Path.Combine(paths);
#else
            return paths.Aggregate(Path.Combine);
#endif
        }

        public static string CombineWithForwardSlashes(params string[] paths)
        {
            return paths.Aggregate(Path.Combine).Replace('\\', '/');
        }

        public static string ConvertToForwardSlashes(string path)
        {
            return CombineWithForwardSlashes(new[] { path });
        }

        public static string NormalizePath(string path)
        {
            var isNetworkSharePath = path.StartsWith(@"\\");
            var slashes = new[] { Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar };
            var parts = path.Split(slashes, StringSplitOptions.RemoveEmptyEntries);
            var stack = new Stack<string>();
            foreach (var part in parts)
            {
                if (part == "..")
                {
                    if (stack.Count > 0)
                    {
                        stack.Pop();
                    }
                    else
                    {
                        throw new ArgumentException("Too many \"..\" in the path \"" + path + "\".");
                    }
                }
                else if (part != ".")
                {
                    stack.Push(part);
                }
            }

            if (isNetworkSharePath)
            {
                return @"\\" + string.Join(@"\", stack.Reverse().ToArray());
            }
            else
            {
                var returnPath = string.Join("/", stack.Reverse().ToArray());
                return (path[0] == '/' ? "/" : "") + returnPath;
            }
        }

        public static bool PathsEqual(string path1, string path2)
        {
            if (path1 == null && path2 == null)
            {
                return true;
            }
            if (path1 == null || path2 == null)
            {
                return false;
            }
            var slashes = new[] {Path.AltDirectorySeparatorChar, Path.DirectorySeparatorChar};
            return path1.Split(slashes).SequenceEqual(path2.Split(slashes), StringComparer.OrdinalIgnoreCase);
        }

        public static string AppRelative(string path)
        {
            if (path.IsUrl()) return path;

            if (!path.StartsWith("~"))
            {
                path = (path.StartsWith("/") ? "~" : "~/") + path;
            }
            return NormalizePath(path);
        }

        /// <summary>
        /// Escape each part of the URL between the slashes.
        /// </summary>
        public static string UriEscapePathSegments(string path)
        {
            return string.Join(
                "/",
                path.Split('/').Select(Uri.EscapeDataString).ToArray()
            );
        }
    }
}