using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using Cassette.BundleProcessing;
using Newtonsoft.Json;

namespace Cassette.Configuration
{
    class FileHelper : IFileHelper
    {
        public void Write(string filename, string writeText)
        {
            File.WriteAllText(filename, writeText);
        }
        
        public string ReadAllText(string filename)
        {
            return File.ReadAllText(filename);
        }

        public bool Exists(string fileName)
        {
            return File.Exists(fileName);
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
                new DirectoryInfo(cacheDirectory).GetFiles().ToList().ForEach(f => f.Delete());
                File.Create(cacheDirectory + cacheVersion);
            }
        }
    }
}
