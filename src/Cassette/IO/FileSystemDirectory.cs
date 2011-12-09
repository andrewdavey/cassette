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
    class FileSystemDirectory : IDirectory
    {
        public FileSystemDirectory(string fullSystemPath)
        {
            this.fullSystemPath = PathUtilities.NormalizePath(fullSystemPath);
        }

        readonly string fullSystemPath;
        FileSystemDirectory parent;

        public string FullPath
        {
            get
            {
                if (parent == null)
                {
                    return "~/";
                }
                else
                {
                    return "~/" + fullSystemPath.Substring(GetRootDirectory().fullSystemPath.Length + 1);
                }
            }
        }

        public IFile GetFile(string filename)
        {
            try
            {
                if (filename.Replace('\\', '/').StartsWith(fullSystemPath))
                {
                    filename = filename.Substring(fullSystemPath.Length + 1);
                }

                var subDirectoryPath = Path.GetDirectoryName(filename);
                var subDirectory = GetDirectory(subDirectoryPath);
                var path = GetAbsolutePath(filename);
                return new FileSystemFile(Path.GetFileName(filename), subDirectory, path);
            }
            catch (DirectoryNotFoundException)
            {
                return new NonExistentFile(filename);
            }
        }

        public IEnumerable<IFile> GetFiles(string searchPattern, SearchOption searchOption)
        {
            return Directory.GetFiles(fullSystemPath, searchPattern, searchOption)
                            .Select(GetFile);
        }

        public bool DirectoryExists(string path)
        {
            return Directory.Exists(GetAbsolutePath(path));
        }

        public void DeleteContents()
        {
            foreach (var directory in Directory.GetDirectories(fullSystemPath))
            {
                Directory.Delete(directory, true);
            }
            foreach (var filename in Directory.GetFiles(fullSystemPath))
            {
                File.Delete(filename);
            }
        }

        string GetAbsolutePath(string filename)
        {
            if (filename == "~")
            {
                return fullSystemPath;
            }
            if (filename.StartsWith("~/"))
            {
                return GetRootDirectory().GetAbsolutePath(filename.Substring(2));
            }

            return PathUtilities.NormalizePath(Path.Combine(fullSystemPath, filename));
        }

        public IDirectory GetDirectory(string path)
        {
            if (path == "") return this;
            if (path[0] == '~')
            {
                path = path.Length == 1 ? "" : path.Substring(2);
                return GetRootDirectory().GetDirectory(path);
            }

            var fullPath = GetAbsolutePath(path);
            if (Directory.Exists(fullPath) == false)
            {
                throw new DirectoryNotFoundException("Directory not found: " + fullPath);
            }
            return new FileSystemDirectory(fullPath)
            {
                parent = this
            };
        }

        public IEnumerable<IDirectory> GetDirectories()
        {
            return Directory.EnumerateDirectories(fullSystemPath)
                            .Select(GetDirectory);
        }

        public FileAttributes Attributes
        {
            get { return File.GetAttributes(fullSystemPath); }
        }

        FileSystemDirectory GetRootDirectory()
        {
            return (parent == null)
                ? this 
                : parent.GetRootDirectory();
        }
    }
}