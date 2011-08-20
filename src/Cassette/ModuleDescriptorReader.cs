using System;
using System.Collections.Generic;
using System.IO;

namespace Cassette
{
    public class ModuleDescriptorReader
    {
        public ModuleDescriptorReader(Stream stream, IEnumerable<string> filenames)
        {
            this.stream = stream;
            allFilenames = new HashSet<string>(filenames, StringComparer.OrdinalIgnoreCase);
        }

        readonly Stream stream;
        readonly HashSet<string> allFilenames;

        public IEnumerable<string> ReadFilenames()
        {
            var filesAdded = new HashSet<string> { "module.txt" };
            using (var reader = new StreamReader(stream))
            {
                string line;
                while ((line = reader.ReadLine()) != null)
                {
                    line = line.Trim();
                    if (string.IsNullOrWhiteSpace(line)) continue;
                    if (line.StartsWith("#")) continue;
                    var commentStart = line.IndexOf('#');
                    if (commentStart >= 0)
                    {
                        line = line.Substring(0, commentStart).TrimEnd();
                    }
                    if (line == "*")
                    {
                        foreach (var filename in allFilenames)
                        {
                            if (filesAdded.Contains(filename)) continue;
                            yield return filename;
                        }
                    }
                    else if (FileExists(line) == false)
                    {
                        throw new FileNotFoundException(
                            "File in module descriptor not found: " + line
                        );
                    }
                    else
                    {
                        filesAdded.Add(line);
                        yield return line;
                    }
                }
            }
        }

        bool FileExists(string filename)
        {
            return allFilenames.Contains(filename);
        }
    }
}