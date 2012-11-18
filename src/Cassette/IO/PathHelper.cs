namespace Cassette.IO
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;
    using System.IO;

    public static class PathHelper
    {
        public static string Combine(string first, params string[] others)
        {
            string path = first;
            foreach (string section in others)
            {
                path = Path.Combine(path, section);
            }
            return path;
        }
    }
}
