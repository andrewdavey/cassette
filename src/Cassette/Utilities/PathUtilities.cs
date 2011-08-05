using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace Cassette.Utilities
{
    public class PathUtilities
    {
        /// <summary>
        /// Traverses a relative path from the given current directory..
        /// </summary>
        /// <param name="currentAbsoluteDirectory">The absolute directory to start traversing from.</param>
        /// <param name="relativePath">The relative path to traverse.</param>
        /// <returns>The normalized, absolute path.</returns>
        public static string NormalizePath(string currentAbsoluteDirectory, string relativePath)
        {
            if (currentAbsoluteDirectory == null)
            {
                throw new ArgumentNullException("currentAbsoluteDirectory");
            }
            if (relativePath == null)
            {
                throw new ArgumentNullException("relativePath");
            }
            if (Path.IsPathRooted(currentAbsoluteDirectory) == false)
            {
                throw new ArgumentException("Current directory must be absolute.", "currentAbsoluteDirectory");
            }
            if (Path.IsPathRooted(relativePath))
            {
                throw new ArgumentException("Path must be relative", "relativePath");
            }

            var slashes = new[] { Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar };
            var parts = currentAbsoluteDirectory.Split(slashes, StringSplitOptions.RemoveEmptyEntries)
                        .Concat(relativePath.Split(slashes, StringSplitOptions.RemoveEmptyEntries));
            var stack = new Stack<string>();
            foreach (var part in parts)
            {
                if (part == "..")
                {
                    if (stack.Count > 1)
                    {
                        stack.Pop();
                    }
                    else
                    {
                        throw new ArgumentException("Too many \"..\" in the path \"" + Path.Combine(currentAbsoluteDirectory, relativePath) + "\".");
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
            return string.Join(Path.DirectorySeparatorChar.ToString(), stack.Reverse());
        }

        public static string EnsureConsistentDirectorySeparators(string path)
        {
            return path.Replace(Path.AltDirectorySeparatorChar, Path.DirectorySeparatorChar);
        }
    }
}
