using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace Cassette.Utilities
{
    public class PathUtilities
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
            return string.Join(Path.DirectorySeparatorChar.ToString(), stack.Reverse());
        }
    }
}
