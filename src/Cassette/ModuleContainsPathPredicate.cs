using System;
using System.IO;
using Cassette.Utilities;

namespace Cassette
{
    public class ModuleContainsPathPredicate : IAssetVisitor
    {
        public bool ModuleContainsPath(string pathRelativeToModuleSource, Module module)
        {
            pathToFind = pathRelativeToModuleSource.IsUrl() ? pathRelativeToModuleSource : NormalizePath(pathRelativeToModuleSource);
            module.Accept(this);
            return isFound;
        }

        string pathToFind;
        bool isFound;
        Module currentModule;

        void IAssetVisitor.Visit(Module module)
        {
            currentModule = module;
            if (module.Path.Equals(pathToFind, StringComparison.OrdinalIgnoreCase))
            {
                isFound = true;
            }
        }

        void IAssetVisitor.Visit(IAsset asset)
        {
            var filename = Path.Combine(currentModule.Path, asset.SourceFilename);
            if (filename.Equals(pathToFind, StringComparison.OrdinalIgnoreCase))
            {
                isFound = true;
            }
        }

        string NormalizePath(string path)
        {
            return path
                .TrimEnd(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar)
                .Replace("\\", "/");
        }
    }
}
