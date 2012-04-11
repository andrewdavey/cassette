using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using Cassette.IO;
using Cassette.Utilities;

namespace Cassette
{
#pragma warning disable 0659 // Don't need to use GetHashCode
    public class FakeFileSystem : IEnumerable, IDirectory
    {
        readonly Dictionary<string, IFile> files;
        FakeFileSystem root;

        public FakeFileSystem()
        {
            FullPath = "~";
            files = new Dictionary<string, IFile>();
        }

        FakeFileSystem(IEnumerable<KeyValuePair<string, IFile>> files)
        {
            this.files = files.ToDictionary(f => f.Key, f => f.Value);
        }

        public void Add(string filename)
        {
            Add(filename, "");
        }

        public void Add(string filename, byte[] bytes)
        {
            Func<IDirectory> directory = () => GetDirectory(filename.Substring(0, filename.LastIndexOf('/')));
            var file = new FakeFile(filename, directory)
            {
                Content = bytes
            };
            files.Add(filename, file);
        }

        public void Add(string filename, string content)
        {
            Add(filename, Encoding.UTF8.GetBytes(content));
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return files.GetEnumerator();
        }

        public FileAttributes Attributes
        {
            get { return FileAttributes.Normal; }
        }

        public string FullPath { get; set; }

        public IFile GetFile(string filename)
        {
            if (filename.StartsWith("~"))
            {
                if (root != null)
                {
                    return root.GetFile(filename);
                }
                else
                {
                    if (files.ContainsKey(filename))
                    {
                        return files[filename];
                    }
                    else
                    {
                        return new NonExistentFile(filename);
                    }
                }
            }
            else
            {
                filename = PathUtilities.NormalizePath(FullPath + "/" + filename);
                return GetFile(filename);
            }
        }

        public IDirectory GetDirectory(string path)
        {
            path = PathUtilities.AppRelative(path);
            return new FakeFileSystem(files.Where(f => f.Key.StartsWith(path)))
            {
                FullPath = path,
                root = root ?? this
            };
        }

        public bool DirectoryExists(string path)
        {
            path = PathUtilities.AppRelative(path);
            return files.Any(f => f.Key.StartsWith(path) && f.Key != path);
        }

        public IEnumerable<IDirectory> GetDirectories()
        {
            var groups = (
                from pair in files
                let secondSlashIndex = pair.Key.IndexOf('/', 2)
                where secondSlashIndex > -1
                select new { pair, secondSlashIndex }
                ).GroupBy(x => x.pair.Key.Substring(0, x.secondSlashIndex));
#if NET35
            return groups.Select(g => new FakeFileSystem(g.Select(x => x.pair))
            {
                FullPath = g.Key,
                root = root ?? this
            }).Cast<IDirectory>();
#else
            return groups.Select(g => new FakeFileSystem(g.Select(x => x.pair))
            {
                FullPath = g.Key,
                root = root ?? this
            });
#endif
        }

        public IEnumerable<IFile> GetFiles(string searchPattern, SearchOption searchOption)
        {
            if (searchPattern == "*.*") return files.Values;

            var extensions = searchPattern.Split(';').Select(e => e.Substring(1)).ToArray();
            return files.Values.Where(
                file => extensions.Contains(Path.GetExtension(file.FullPath))
            );
        }

        public IDisposable WatchForChanges(Action<string> pathChanged)
        {
            throw new NotImplementedException();
        }

        public override bool Equals(object obj)
        {
            return FullPath == ((FakeFileSystem)obj).FullPath;
        }
    }
#pragma warning restore 0659

    public class FakeFile : IFile
    {
        readonly Func<IDirectory> getDirectory;

        public FakeFile(string fullPath, Func<IDirectory> directory)
        {
            FullPath = fullPath;
            getDirectory = directory;
            Exists = true;
        }

        public byte[] Content { get; set; }

        public IDirectory Directory
        {
            get { return getDirectory(); }
        }

        public bool Exists { get; set; }

        public DateTime LastWriteTimeUtc
        {
            get { throw new NotImplementedException(); }
        }

        public string FullPath { get; set; }

        public Stream Open(FileMode mode, FileAccess access, FileShare fileShare)
        {
            if (Content == null) return Stream.Null;
            return new MemoryStream(Content);
        }

        public void Delete()
        {
            throw new NotImplementedException();
        }
    }
}