using System.Collections.Generic;
using System.IO;

namespace Cassette
{
    public class ModuleDescriptorReader
    {
        public ModuleDescriptorReader(Stream stream, IFileSystem directory)
        {
            this.stream = stream;
            this.directory = directory;
        }

        readonly Stream stream;
        readonly IFileSystem directory;

        public IEnumerable<string> ReadFilenames()
        {
            var filesAdded = new HashSet<string>();
            filesAdded.Add("module.txt");
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
                        foreach (var filename in directory.GetFiles(""))
                        {
                            if (filesAdded.Contains(filename)) continue;
                            yield return filename;
                        }
                    }
                    else if (directory.FileExists(line) == false)
                    {
                        throw new FileNotFoundException("File in module descriptor not found: " + line, directory.GetAbsolutePath(line));
                    }
                    else
                    {
                        filesAdded.Add(line);
                        yield return line;
                    }
                }
            }
        }
    }
}