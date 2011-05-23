using System.Collections.Generic;
using System.IO;
using System.IO.IsolatedStorage;
using System.Linq;

namespace Knapsack
{
    public abstract class ModuleContainerBuilder
    {
        protected readonly IsolatedStorageFile storage;
        protected readonly string rootDirectory;
        protected readonly List<string> relativeModuleDirectories = new List<string>();

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

        public void AddModule(string directoryRelativeToRootDirectory)
        {
            relativeModuleDirectories.Add(directoryRelativeToRootDirectory.Replace('\\', '/'));
        }

        public void AddModuleForEachSubdirectoryOf(string directoryRelativeToRootDirectory)
        {
            var fullPath = rootDirectory + directoryRelativeToRootDirectory;
            if (Directory.Exists(fullPath))
            {
                foreach (var path in Directory.GetDirectories(fullPath))
                {
                    var info = new DirectoryInfo(path);
                    if (!info.Attributes.HasFlag(FileAttributes.Hidden))
                    {
                        AddModule(path.Substring(rootDirectory.Length));
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
