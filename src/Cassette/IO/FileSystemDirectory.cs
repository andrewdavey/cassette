using System.Collections.Generic;
using System.IO;
using System.Linq;
using Cassette.Utilities;

namespace Cassette.IO
{
    public class FileSystemDirectory : IDirectory
    {
        public FileSystemDirectory(string rootDirectory)
        {
            this.rootDirectory = rootDirectory.TrimEnd('\\', '/');
        }

        readonly string rootDirectory;

        public IFile GetFile(string filename)
        {
            try
            {
                var subDirectoryPath = Path.GetDirectoryName(filename);
                var subDirectory = NavigateTo(subDirectoryPath, false);
                var path = GetAbsolutePath(filename);
                if (File.Exists(path))
                {
                    return new FileSystemFile(path, subDirectory);
                }
                else
                {
                    return new NonExistentFile(path);
                }
            }
            catch (DirectoryNotFoundException)
            {
                return new NonExistentFile(filename);
            }
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

        string GetAbsolutePath(string filename)
        {
            return PathUtilities.NormalizePath(PathUtilities.CombineWithForwardSlashes(rootDirectory, filename));
        }

        string ToRelativePath(string fullPath)
        {
            return fullPath.Substring(rootDirectory.Length + 1);
        }

        public IDirectory NavigateTo(string path, bool createIfNotExists)
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
            return new FileSystemDirectory(fullPath);
        }

        public IEnumerable<string> GetDirectories(string relativePath)
        {
            return Directory.EnumerateDirectories(GetAbsolutePath(relativePath)).Select(ToRelativePath);
        }

        public IEnumerable<string> GetFiles(string directory, SearchOption searchOption)
        {
            return Directory.GetFiles(GetAbsolutePath(directory), "*", searchOption).Select(ToRelativePath);
        }

        public IEnumerable<string> GetFiles(string directory, SearchOption searchOption, string searchPattern)
        {
            return Directory.GetFiles(GetAbsolutePath(directory), searchPattern, searchOption).Select(ToRelativePath);
        }

        public FileAttributes GetAttributes(string path)
        {
            return File.GetAttributes(GetAbsolutePath(path));
        }
    }
}