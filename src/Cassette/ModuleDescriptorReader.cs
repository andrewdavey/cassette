using System;
using System.Collections.Generic;
using System.IO;

namespace Cassette
{
    public class ModuleDescriptorReader
    {
        public ModuleDescriptorReader(Stream stream, IEnumerable<string> allAssetfilenames)
        {
            this.stream = stream;
            this.allAssetfilenames = new HashSet<string>(allAssetfilenames, StringComparer.OrdinalIgnoreCase);
            sectionLineParsers = new Dictionary<string, Action<string>>
            {
                {"assets", ParseAsset},
                {"references", ParseReference}
            };
        }

        readonly Stream stream;
        readonly HashSet<string> allAssetfilenames;
        readonly SortedSet<string> assetFilenames = new SortedSet<string>();
        readonly HashSet<string> references = new HashSet<string>(); 
        readonly Dictionary<string, Action<string>> sectionLineParsers;
        string currentSection = "assets";

        public ModuleDescriptor Read()
        {
            using (var reader = new StreamReader(stream))
            {
                string line;
                while ((line = reader.ReadLine()) != null)
                {
                    ProcessLine(line);
                }
            }
            return new ModuleDescriptor(assetFilenames, references);
        }

        void ProcessLine(string line)
        {
            line = line.Trim();
            if (string.IsNullOrWhiteSpace(line)) return;
            if (IsComment(line)) return;
            line = RemoveTrailingComment(line);
            if (DetermineSection(line)) return;
            sectionLineParsers[currentSection](line);
        }

        bool DetermineSection(string line)
        {
            if (line.StartsWith("["))
            {
                currentSection = line.Substring(1).TrimEnd(']');
                if (sectionLineParsers.ContainsKey(currentSection))
                {
                    return true;
                }
                else
                {
                    throw new Exception(string.Format("Unexpected module descriptor section \"{0}\".", line));
                }
            }
            return false;
        }

        bool IsComment(string line)
        {
            return line.StartsWith("#");
        }

        void ParseAsset(string line)
        {
            if (line == "*")
            {
                foreach (var filename in allAssetfilenames)
                {
                    assetFilenames.Add(filename);
                }
            }
            else if (FileExists(line))
            {
                if (assetFilenames.Contains(line))
                {
                    throw new Exception(string.Format("The file \"{0}\" cannot appear twice in module descriptor.", line));
                }
                assetFilenames.Add(line);
            }
            else
            {
                throw new FileNotFoundException(string.Format(
                    "Cannot find the file \"{0}\", referenced by module descriptor.",
                    line
                ));
            }
        }

        void ParseReference(string line)
        {
            references.Add(line);
        }

        string RemoveTrailingComment(string line)
        {
            var commentStart = line.IndexOf('#');
            if (commentStart >= 0)
            {
                line = line.Substring(0, commentStart).TrimEnd();
            }
            return line;
        }

        bool FileExists(string filename)
        {
            return allAssetfilenames.Contains(filename);
        }
    }
}