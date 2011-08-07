using System;
using System.IO;

namespace Cassette
{
    public class ModuleContainsPathPredicate : IAssetVisitor
    {
        public bool ModuleContainsPath(string pathRelativeToModuleSource, Module module)
        {
            pathToFind = pathRelativeToModuleSource.TrimEnd(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar);
            module.Accept(this);
            return isFound;
        }

        string pathToFind;
        bool isFound;
        Module currentModule;

        void IAssetVisitor.Visit(Module module)
        {
            currentModule = module;
            if (module.Directory.Equals(pathToFind, StringComparison.OrdinalIgnoreCase))
            {
                isFound = true;
            }
        }

        void IAssetVisitor.Visit(IAsset asset)
        {
            var filename = Path.Combine(currentModule.Directory, asset.SourceFilename);
            if (filename.Equals(pathToFind, StringComparison.OrdinalIgnoreCase))
            {
                isFound = true;
            }
        }
    }
}
