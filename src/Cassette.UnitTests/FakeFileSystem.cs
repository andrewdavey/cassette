using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Cassette.IO;

namespace Cassette
{
    public class FakeFileSystem : IEnumerable, IDirectory
    {
        readonly Dictionary<string, IFile> files;

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
            Add(filename, null);
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

        IEnumerator IEnumerable.GetEnumerator()
        {
            return files.GetEnumerator();
        }

        public FileAttributes Attributes
        {
            get { throw new System.NotImplementedException(); }
        }

        public string FullPath { get; set; }

        public IFile GetFile(string filename)
        {
            if (!filename.StartsWith("~"))
            {
                filename = FullPath + "/" + filename;
            }
            if (files.ContainsKey(filename))
            {
                return files[filename];
            }
            else
            {
                return new NonExistentFile(filename);
            }
        }

        public IDirectory GetDirectory(string path)
        {
            return new FakeFileSystem(files.Where(f => f.Key.StartsWith(path)))
            {
                FullPath = path
            };
        }

        public bool DirectoryExists(string path)
        {
            throw new System.NotImplementedException();
        }

        public IEnumerable<IDirectory> GetDirectories()
        {
            throw new System.NotImplementedException();
        }

        public IEnumerable<IFile> GetFiles(string searchPattern, SearchOption searchOption)
        {
            throw new System.NotImplementedException();
        }
    }

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