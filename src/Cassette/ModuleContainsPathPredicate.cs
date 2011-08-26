using System.IO;
using Cassette.Utilities;

namespace Cassette
{
    public class ModuleContainsPathPredicate : IAssetVisitor
    {
        public bool ModuleContainsPath(string path, Module module)
        {
            pathToFind = path.IsUrl() ? path : NormalizePath(path, module);
            module.Accept(this);
            return isFound;
        }

        string pathToFind;
        bool isFound;
        Module currentModule;

        void IAssetVisitor.Visit(Module module)
        {
            currentModule = module;
            if (IsMatch(module.Path))
            {
                isFound = true;
            }
        }

        void IAssetVisitor.Visit(IAsset asset)
        {
            var filename = Path.Combine(currentModule.Path, asset.SourceFilename);
            if (IsMatch(filename))
            {
                isFound = true;
            }
        }

        string NormalizePath(string path, Module module)
        {
            path = path.TrimEnd(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar);
            if (path.StartsWith("~"))
            {
                return path.Substring(2);
            }
            else
            {
                return Path.Combine(module.Path, path);
            }
        }

        bool IsMatch(string path)
        {
            return PathUtilities.PathsEqual(path, pathToFind);
        }
    }
}
