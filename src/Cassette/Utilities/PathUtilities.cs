﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace Cassette.Utilities
{
    static class PathUtilities
    {
        public static string Combine(params string[] paths)
        {
#if NET40
            return Path.Combine(paths);
#endif
#if NET35
            return paths.Aggregate((p1, p2) => Path.Combine(p1, p2));
#endif
        }
    
        public static string CombineWithForwardSlashes(params string[] paths)
        {
            return paths.Aggregate((a, b) => Path.Combine(a, b)).Replace('\\', '/');
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
                return string.Join("/", stack.Reverse().ToArray());
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
    }
}