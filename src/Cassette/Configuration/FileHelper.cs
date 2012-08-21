using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using Cassette.BundleProcessing;
using Cassette.IO;
using Newtonsoft.Json;

namespace Cassette.Configuration
{
    class FileHelper : IFileHelper
    {
        static readonly object locker = new object();

        public void Write(string filename, string writeText)
        {
            File.WriteAllText(filename, writeText);
        }
        
        public string ReadAllText(string filename)
        {
            return File.ReadAllText(filename);
        }

        public void CreateDirectory(string directory)
        {
            Directory.CreateDirectory(directory);
        }

        public bool Exists(string fileName)
        {
            return File.Exists(fileName);
        }

        public IFile GetFileSystemFile(IDirectory directory, string systemAbsoluteFilename, 
            string cacheDirectory)
        {
            return new FileSystemFile(Path.GetFileName(systemAbsoluteFilename),
                    directory.GetDirectory(Path.GetDirectoryName(systemAbsoluteFilename)),
                    systemAbsoluteFilename);
        }

        public DateTime GetLastAccessTime(string filename)
        {
            return File.GetLastAccessTime(filename);
        }

        public void Delete(string fileName)
        {
            File.Delete(fileName);
        }

        public void PrepareCachingDirectory(string cacheDirectory, string cacheVersion)
        {
            if (!Directory.Exists(cacheDirectory))
            {
                Directory.CreateDirectory(cacheDirectory);
            }
            else if (Directory.GetLastWriteTime(cacheDirectory).Date < DateTime.Today.Date ||
                !File.Exists(cacheDirectory + cacheVersion))
            {
                lock (locker)
                {
                    new DirectoryInfo(cacheDirectory).GetFiles("*", SearchOption.AllDirectories)
                        .ToList().ForEach(f => f.Delete());
                    File.Create(cacheDirectory + cacheVersion);
                }
            }
        }
    }
}
