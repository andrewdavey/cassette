using System;
using System.Collections.Generic;
using System.IO;
using System.IO.IsolatedStorage;
using System.Linq;
using Storage = System.IO.IsolatedStorage.IsolatedStorageFile;

namespace Cassette.IO
{
    public class IsolatedStorageFile : IFile
    {
        readonly string filename;
        readonly Func<Storage> getStorage;
        readonly IsolatedStorageDirectory directory;
        readonly string systemFilename;
        const string IndexFilename = "index.cassette";

        public IsolatedStorageFile(string filename, Storage storage, IsolatedStorageDirectory directory)
            : this(filename, () => storage, directory)
        {
        }

        public IsolatedStorageFile(string filename, Func<Storage> getStorage, IsolatedStorageDirectory directory)
        {
            this.filename = filename;
            this.getStorage = getStorage;
            this.directory = directory;
            systemFilename = filename.Substring(2); // Skip the "~/" prefix.

            // Build index for this file, if it doesn't already exist
            WriteLastWriteTimeUtc();
        }

        public IDirectory Directory
        {
            get { return directory; }
        }

        public string FullPath
        {
            get { return filename; }
        }

        public Stream Open(FileMode mode, FileAccess access, FileShare fileShare)
        {
            // FX35: Use index file to store write times
            if (access == FileAccess.Write)
            {
                WriteLastWriteTimeUtc();
            }

            return new IsolatedStorageFileStream(systemFilename, mode, access, fileShare, Storage);
        }

        public bool Exists
        {
            get
            {
                return Storage.GetFileNames(systemFilename).Length > 0;
            }
        }

        public DateTime LastWriteTimeUtc
        {
            get
            {
                // FX35
                return ReadLastWriteTimeUtc();

                // FX40: return Storage.GetLastWriteTime(systemFilename).UtcDateTime;
            }
        }

        public void Delete()
        {
            // FX35: Remove from index
            RemoveFromIndex();

            Storage.DeleteFile(systemFilename);
        }

        Storage Storage
        {
            get { return getStorage(); }
        }

        #region FX35 Helpers

        private DateTime ReadLastWriteTimeUtc()
        {
            long writeTime;
            if (GetIndex().TryGetValue(systemFilename, out writeTime))
            {
                return DateTime.FromFileTimeUtc(writeTime);
            }
            else
            {
                throw new IOException("Could not find last write time for " + systemFilename);
            }
        }

        private FileIndex GetIndex()
        {
            FileIndex index;

            using (var reader = new StreamReader(OpenIndexFile(FileMode.OpenOrCreate, FileAccess.Read)))
            {
                index = new FileIndex(reader.ReadToEnd());
            }

            return index;
        }

        private void WriteLastWriteTimeUtc()
        {
            var index = GetIndex();

            // Open writing
            using (var writer = new StreamWriter(OpenIndexFile(FileMode.OpenOrCreate, FileAccess.ReadWrite)))
            {
                if (index.ContainsKey(systemFilename))
                {
                    // Update existing
                    index[systemFilename] = DateTime.UtcNow.ToFileTimeUtc();
                }
                else
                {
                    // Append
                    index.Add(systemFilename, DateTime.UtcNow.ToFileTimeUtc());
                }

                writer.Write(index.ToString());
            }
        }

        private void RemoveFromIndex()
        {
            var index = GetIndex();

            if (index.ContainsKey(systemFilename))
            {
                index.Remove(systemFilename);

                using (var writer = new StreamWriter(OpenIndexFile(FileMode.Open, FileAccess.Write)))
                {
                    writer.Write(index.ToString());
                }
            }
        }

        private Stream OpenIndexFile(FileMode mode, FileAccess access)
        {
            return new IsolatedStorageFileStream(IndexFilename, mode, access, Storage);
        }

        private class FileIndex : Dictionary<string, long>
        {

            /// <summary>
            /// Reads in a text file contents and parses into dictionary
            /// </summary>
            /// <param name="textFile"></param>
            public FileIndex(string textFile)
            {
                var lines = textFile.Split(new []{'\n'}, StringSplitOptions.RemoveEmptyEntries);

                foreach (var line in lines)
                {
                    var pair = line.Split('|');

                    if (pair.Length == 2)
                    {
                        this.Add(pair[0], Convert.ToInt64(pair[1]));
                    }                    
                }
            }

            /// <summary>
            /// Writes index to a string for serialization
            /// </summary>
            /// <returns></returns>
            public override string ToString()
            {
                return String.Join("\n", this.Select(kv => kv.Key + "|" + kv.Value).ToArray());
            }
        }

        #endregion
    }
}