using System;
using System.Linq;
using System.IO;
using System.Collections.Generic;

namespace Cassette
{
    public class FileSystem : IFileSystem
    {
        public FileSystem(string rootDirectory)
        {
            this.rootDirectory = rootDirectory.TrimEnd('\\', '/');
        }

        readonly string rootDirectory;

        public Stream OpenFile(string filename, FileMode mode, FileAccess access)
        {
            return File.Open(GetFullPath(filename), mode, access);
        }

        public bool FileExists(string filename)
        {
            return File.Exists(GetFullPath(filename));
        }

        public bool DirectoryExists(string filename)
        {
            return Directory.Exists(GetFullPath(filename));
        }

        public void DeleteAll()
        {
            foreach (var directory in Directory.GetDirectories(rootDirectory))
            {
                Directory.Delete(directory, true);
            }
            foreach (var filename in Directory.GetFiles(rootDirectory))
            {
                File.Delete(filename);
            }
        }

        public DateTime GetLastWriteTimeUtc(string filename)
        {
            return File.GetLastWriteTimeUtc(filename);
        }

        string GetFullPath(string filename)
        {
            return Path.Combine(rootDirectory, filename);
        }

        string ToRelativePath(string fullPath)
        {
            return fullPath.Substring(rootDirectory.Length + 1);
        }

        public IFileSystem AtSubDirectory(string path, bool createIfNotExists)
        {
            var fullPath = GetFullPath(path);
            if (Directory.Exists(fullPath) == false)
            {
                if (createIfNotExists)
                {
                    Directory.CreateDirectory(fullPath);
                }
                else
                {
                    throw new DirectoryNotFoundException("Directory not found: " + fullPath);
                }
            }
            return new FileSystem(fullPath);
        }

        public IEnumerable<string> GetDirectories(string relativePath)
        {
            return Directory.EnumerateDirectories(GetFullPath(relativePath)).Select(ToRelativePath);
        }

        public IEnumerable<string> GetFiles(string directory)
        {
            return Directory.GetFiles(GetFullPath(directory), "*", SearchOption.AllDirectories).Select(ToRelativePath);
        }

        public IEnumerable<string> GetFiles(string directory, string searchPattern)
        {
            return Directory.GetFiles(GetFullPath(directory), searchPattern, SearchOption.AllDirectories).Select(ToRelativePath);
        }

        public FileAttributes GetAttributes(string path)
        {
            return File.GetAttributes(GetFullPath(path));
        }
    }
}
