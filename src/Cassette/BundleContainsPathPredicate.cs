using System.IO;
using Cassette.Utilities;

namespace Cassette
{
    class BundleContainsPathPredicate : IBundleVisitor
    {
        public bool BundleContainsPath(string path, Bundle bundle)
        {
            pathToFind = path.IsUrl() ? path : NormalizePath(path, bundle);
            bundle.Accept(this);
            return isFound;
        }

        string pathToFind;
        bool isFound;

        void IBundleVisitor.Visit(Bundle bundle)
        {
            if (IsMatch(bundle.Path))
            {
                isFound = true;
            }
        }

        void IBundleVisitor.Visit(IAsset asset)
        {
            if (IsMatch(asset.SourceFile.FullPath))
            {
                isFound = true;
            }
        }

        string NormalizePath(string path, Bundle bundle)
        {
            path = path.TrimEnd(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar);
            if (path.StartsWith("~"))
            {
                return path;
            }
            else
            {
                return PathUtilities.CombineWithForwardSlashes(bundle.Path, path);
            }
        }

        bool IsMatch(string path)
        {
            return PathUtilities.PathsEqual(path, pathToFind);
        }
    }
}

