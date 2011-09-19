#region License
/*
Copyright 2011 Andrew Davey

This file is part of Cassette.

Cassette is free software: you can redistribute it and/or modify it under the 
terms of the GNU General Public License as published by the Free Software 
Foundation, either version 3 of the License, or (at your option) any later 
version.

Cassette is distributed in the hope that it will be useful, but WITHOUT ANY 
WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS 
FOR A PARTICULAR PURPOSE. See the GNU General Public License for more details.

You should have received a copy of the GNU General Public License along with 
Cassette. If not, see http://www.gnu.org/licenses/.
*/
#endregion

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
                var subDirectory = GetDirectory(subDirectoryPath, false);
                var path = GetAbsolutePath(filename);
                return new FileSystemFile(path, subDirectory);
            }
            catch (DirectoryNotFoundException)
            {
                return new NonExistentFile(filename);
            }
        }

        public void DeleteContents()
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

        public IDirectory GetDirectory(string path, bool createIfNotExists)
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

        public IEnumerable<string> GetDirectoryPaths(string relativePath)
        {
            return Directory.EnumerateDirectories(GetAbsolutePath(relativePath)).Select(ToRelativePath);
        }

        public IEnumerable<string> GetFilePaths(string directory, SearchOption searchOption, string searchPattern)
        {
            return Directory.GetFiles(GetAbsolutePath(directory), searchPattern, searchOption).Select(ToRelativePath);
        }

        public FileAttributes GetAttributes(string path)
        {
            return File.GetAttributes(GetAbsolutePath(path));
        }
    }
}
