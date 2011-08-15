using System.Collections.Generic;

namespace Cassette
{
    public class PerSubDirectorySource<T> : FileSystemModuleSource<T>
        where T : Module
    {
        public PerSubDirectorySource(string baseDirectory)
        {
            this.baseDirectory = baseDirectory;
        }

        readonly string baseDirectory;

        protected override IEnumerable<string> GetModuleDirectoryPaths(ICassetteApplication application)
        {
            return application.RootDirectory.GetDirectories(baseDirectory);
        }
    }
}