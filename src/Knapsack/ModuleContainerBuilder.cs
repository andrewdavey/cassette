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
            this.rootDirectory = EnsureRootDirectoryEndsWithSlash(rootDirectory);
        }

        string EnsureRootDirectoryEndsWithSlash(string rootDirectory)
        {
            var last = rootDirectory.Last();
            if (last != Path.DirectorySeparatorChar && last != Path.AltDirectorySeparatorChar)
            {
                rootDirectory += "/";
            }
            return rootDirectory;
        }

        public void AddModule(string directoryRelativeToRootDirectory)
        {
            relativeModuleDirectories.Add(directoryRelativeToRootDirectory.Replace('\\', '/'));
        }

        public void AddModuleForEachSubdirectoryOf(string directoryRelativeToRootDirectory)
        {
            var fullPath = rootDirectory + directoryRelativeToRootDirectory;
            foreach (var path in Directory.GetDirectories(fullPath))
            {
                AddModule(path.Substring(rootDirectory.Length));
            }
        }

        protected string LoadFile(string relativeFilename)
        {
            return File.ReadAllText(Path.Combine(rootDirectory, relativeFilename));
        }
    }
}
