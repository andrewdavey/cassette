using System.Collections.Generic;

namespace Cassette
{
    public class DirectorySource<T> : FileSystemModuleSource<T>
        where T : Module
    {
        public DirectorySource(params string[] relativeDirectoryPaths)
        {
            this.relativeDirectoryPaths = relativeDirectoryPaths;
        }

        readonly string[] relativeDirectoryPaths;

        protected override IEnumerable<string> GetModuleDirectoryPaths(ICassetteApplication application)
        {
            return relativeDirectoryPaths;
        }
    }
}
