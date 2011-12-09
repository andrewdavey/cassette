#region License
/*
Copyright 2011 Andrew Davey

This file is part of Cassette.

Cassette is free software: you can redistribute it and/or modify it under the 
terms of the GNU General Public License as published by the Free Software 
Foundation, either version 3 of the License, or (at your option) any later 
version.

Cassette is distributed in the hope that it will be useful, but WITHOUT ANY 
WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS 
FOR A PARTICULAR PURPOSE. See the GNU General Public License for more details.

You should have received a copy of the GNU General Public License along with 
Cassette. If not, see http://www.gnu.org/licenses/.
*/
#endregion

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace Cassette.Utilities
{
    static class PathUtilities
    {
        public static string CombineWithForwardSlashes(params string[] paths)
        {
            return Path.Combine(paths).Replace('\\', '/');
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
                else if (part == ".")
                {
                    continue;
                }
                else
                {
                    stack.Push(part);
                }
            }
            if (isNetworkSharePath)
            {
                return @"\\" + string.Join(@"\", stack.Reverse());
            }
            else
            {
                return string.Join("/", stack.Reverse());
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