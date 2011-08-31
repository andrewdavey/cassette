using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace Cassette.Utilities
{
    public static class PathUtilities
    {
        public static string NormalizePath(string path)
        {
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
                else if (part == ".")
                {
                    continue;
                }
                else
                {
                    stack.Push(part);
                }
            }
            return string.Join("/", stack.Reverse());
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
    }
}
