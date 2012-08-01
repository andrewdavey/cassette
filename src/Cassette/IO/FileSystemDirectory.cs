using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Cassette.Utilities;

namespace Cassette.IO
{
    public class FileSystemDirectory : IDirectory
    {
        public FileSystemDirectory(string fullSystemPath)
        {
            this.fullSystemPath = PathUtilities.NormalizePath(fullSystemPath);
        }

        readonly string fullSystemPath;
        FileSystemDirectory parent;

        internal string FullSystemPath
        {
            get { return fullSystemPath; }
        }

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

        public bool Exists
        {
            get { return Directory.Exists(fullSystemPath); }
        }

        public IFile GetFile(string filename)
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

        public IEnumerable<IFile> GetFiles(string searchPattern, SearchOption searchOption)
        {
            return Directory.GetFiles(fullSystemPath, searchPattern, searchOption)
                            .Select(GetFile);
        }

        public bool DirectoryExists(string path)
        {
            return Directory.Exists(GetAbsolutePath(path));
        }

        public void Delete()
        {
            Directory.Delete(fullSystemPath, true);
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
            return new FileSystemDirectory(fullPath)
            {
                parent = this
            };
        }

        public IEnumerable<IDirectory> GetDirectories()
        {
#if NET35
            return Directory.GetDirectories(fullSystemPath).Select(GetDirectory);
#else
            return Directory.EnumerateDirectories(fullSystemPath).Select(GetDirectory);
#endif
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

        public void Create()
        {
            Directory.CreateDirectory(fullSystemPath);
        }

        public IDisposable WatchForChanges(Action<string> pathCreated, Action<string> pathChanged, Action<string> pathDeleted, Action<string, string> pathRenamed)
        {
            var watcher = new FileSystemWatcher(fullSystemPath)
            {
                IncludeSubdirectories = true
            };

            watcher.Created += (s, e) => pathCreated(ConvertSystemPathToAppPath(e.FullPath));
            watcher.Deleted += (s, e) => pathDeleted(ConvertSystemPathToAppPath(e.FullPath));
            watcher.Changed += (s, e) => pathChanged(ConvertSystemPathToAppPath(e.FullPath));
            watcher.Renamed += (s, e) => pathRenamed(ConvertSystemPathToAppPath(e.OldFullPath), ConvertSystemPathToAppPath(e.FullPath));
            
            watcher.EnableRaisingEvents = true;
            return watcher;
        }

        string ConvertSystemPathToAppPath(string fullPath)
        {
            return "~/" + fullPath.Substring(fullSystemPath.Length).TrimStart('\\', '/').Replace('\\', '/');
        }

        /// <remarks>
        /// This method is a bit of a hack. An independently created FileSystemDirectory could be a sub-directory.
        /// This method converts it to a proper sub-directory object if possible, otherwise returns null.
        /// </remarks>
        internal IDirectory TryGetAsSubDirectory(FileSystemDirectory directory)
        {
            // Example:
            // fullSystemPath == "c:\example"
            // directory.fullSystemPath == "c:\example\sub"
            // return GetDirectory("sub")

            var isSubDirectory = 
                directory.fullSystemPath.Length >= fullSystemPath.Length &&
                directory.fullSystemPath.Substring(0, fullSystemPath.Length).Equals(fullSystemPath, StringComparison.OrdinalIgnoreCase);

            if (isSubDirectory)
            {
                var subPath = directory.fullSystemPath.Substring(fullSystemPath.Length + 1);
                return GetDirectory(subPath);
            }
            else
            {
                return null;
            }
        }
    }
}