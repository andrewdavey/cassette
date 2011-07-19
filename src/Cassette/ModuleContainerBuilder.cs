using System.Collections.Generic;
using System.IO;
using System.IO.IsolatedStorage;
using System.Linq;
using System;

namespace Cassette
{
    public abstract class ModuleContainerBuilder
    {
        protected readonly IsolatedStorageFile storage;
        protected readonly string rootDirectory;
        protected readonly List<Tuple<string, string>> relativeModuleDirectories = new List<Tuple<string, string>>();

        public ModuleContainerBuilder(IsolatedStorageFile storage, string rootDirectory)
        {
            this.storage = storage;
            this.rootDirectory = EnsureDirectoryEndsWithSlash(rootDirectory);
        }

        protected string EnsureDirectoryEndsWithSlash(string directory)
        {
            var last = directory.Last();
            if (last != Path.DirectorySeparatorChar && last != Path.AltDirectorySeparatorChar)
            {
                directory += "/";
            }
            return directory;
        }

        public void AddModule(string directoryRelativeToRootDirectory, string location)
        {
            relativeModuleDirectories.Add(Tuple.Create(directoryRelativeToRootDirectory.Replace('\\', '/'), location));
        }

        public void AddModuleForEachSubdirectoryOf(string directoryRelativeToRootDirectory, string location)
        {
            var fullPath = rootDirectory + directoryRelativeToRootDirectory;
            if (Directory.Exists(fullPath))
            {
                foreach (var path in Directory.GetDirectories(fullPath))
                {
                    var info = new DirectoryInfo(path);
                    if (!info.Attributes.HasFlag(FileAttributes.Hidden))
                    {
                        AddModule(path.Substring(rootDirectory.Length), location);
                    }
                }
            }
        }

        public abstract ModuleContainer Build();

        protected string LoadFile(string relativeFilename)
        {
            return File.ReadAllText(Path.Combine(rootDirectory, relativeFilename));
        }
    }
}
