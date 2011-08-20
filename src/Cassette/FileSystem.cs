using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Cassette.Utilities;

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
            return File.Open(GetAbsolutePath(filename), mode, access);
        }

        public bool FileExists(string filename)
        {
            return File.Exists(GetAbsolutePath(filename));
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
            return File.GetLastWriteTimeUtc(GetAbsolutePath(filename));
        }

        public string GetAbsolutePath(string filename)
        {
            return PathUtilities.NormalizePath(Path.Combine(rootDirectory, filename));
        }

        string ToRelativePath(string fullPath)
        {
            return fullPath.Substring(rootDirectory.Length + 1);
        }

        public IFileSystem NavigateTo(string path, bool createIfNotExists)
        {
            var fullPath = GetAbsolutePath(path);
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
            return Directory.EnumerateDirectories(GetAbsolutePath(relativePath)).Select(ToRelativePath);
        }

        public IEnumerable<string> GetFiles(string directory)
        {
            return Directory.GetFiles(GetAbsolutePath(directory), "*", SearchOption.AllDirectories).Select(ToRelativePath);
        }

        public IEnumerable<string> GetFiles(string directory, string searchPattern)
        {
            return Directory.GetFiles(GetAbsolutePath(directory), searchPattern, SearchOption.AllDirectories).Select(ToRelativePath);
        }

        public FileAttributes GetAttributes(string path)
        {
            return File.GetAttributes(GetAbsolutePath(path));
        }
    }
}
