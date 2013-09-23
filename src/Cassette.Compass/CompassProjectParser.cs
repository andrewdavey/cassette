using System;
using System.IO;

namespace Cassette.Compass
{
    public class CompassProjectParser
    {
        readonly string compassProjectPath;
        readonly string compassProjectDirectory;
        private readonly string virtualCompassProjectDirectory;

        public CompassProjectParser(string compassProjectDirectory, string virtualCompassProjectDirectory)
        {
            this.virtualCompassProjectDirectory = virtualCompassProjectDirectory;
            this.compassProjectDirectory = compassProjectDirectory;

            var compassProjectPath = Path.Combine(compassProjectDirectory, "config.rb");

            if (!File.Exists(compassProjectPath))
                throw new ArgumentException("Compass project path did not exist!", "compassProjectPath");

            this.compassProjectPath = compassProjectPath;
        }

        public CompassProject Parse()
        {
            var lines = File.ReadAllLines(compassProjectPath);
            var result = new CompassProject(compassProjectPath, compassProjectDirectory);

            foreach (var line in lines)
            {
                if (line.StartsWith("css_dir"))
                {
                    var value = GetCompassConfigLineValue(line);
                    result.VirtualCssDirectory = MakeVirtualPath(value);
                    result.PhysicalCssDirectory = MakeAbsolutePath(value);
                }

                if (line.StartsWith("sass_dir"))
                {
                    var value = GetCompassConfigLineValue(line);
                    result.VirtualSassDirectory = MakeVirtualPath(value);
                    result.PhysicalSassDirectory = MakeAbsolutePath(value);
                }
                    

                if (!string.IsNullOrEmpty(result.PhysicalSassDirectory) && !string.IsNullOrEmpty(result.PhysicalCssDirectory))
                    break;
            }

            return result;
        }

        private string GetCompassConfigLineValue(string line)
        {
            return line.Substring(line.IndexOf('"') + 1).TrimEnd('"');
        }

        private string MakeAbsolutePath(string compassRelativePath)
        {
            return Path.Combine(compassProjectDirectory, compassRelativePath.Replace('/', Path.DirectorySeparatorChar));
        }

        private string MakeVirtualPath(string compassRelativePath)
        {
            if (!compassRelativePath.StartsWith("/"))
                return virtualCompassProjectDirectory + "/" + compassRelativePath;

            return compassRelativePath;
        }
    }
}
