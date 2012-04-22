using System;
using System.IO;
using Cassette.Utilities;

namespace Cassette
{
    class BundleContainsPathPredicate : IBundleVisitor
    {
        public BundleContainsPathPredicate(string applicationRelativePath)
        {
            if (!applicationRelativePath.StartsWith("~"))
            {
                throw new ArgumentException("Path must be application relative.", "applicationRelativePath");
            }

            pathToFind = applicationRelativePath;
        }

        public BundleContainsPathPredicate()
        {
        }

        public bool BundleContainsPath(string path, Bundle bundle)
        {
            pathToFind = path.IsUrl() ? path : NormalizePath(path, bundle);
            bundle.Accept(this);
            return isFound;
        }

        string pathToFind;
        bool isFound;

        public bool AllowPartialAssetPaths { get; set; }

        public bool Result
        {
            get { return isFound; }
        }

        void IBundleVisitor.Visit(Bundle bundle)
        {
            if (IsMatch(bundle.Path))
            {
                isFound = true;
            }
        }

        void IBundleVisitor.Visit(IAsset asset)
        {
            if (IsMatch(asset.Path) || (AllowPartialAssetPaths && IsPartialAssetPathMatch(asset.Path)))
            {
                isFound = true;
            }
        }

        /// <summary>
        /// Looking for "~/bundle/sub" can match "~/bundle/sub/asset.js"
        /// </summary>
        bool IsPartialAssetPathMatch(string assetPath)
        {
            if (assetPath.Length < pathToFind.Length) return false;

            var partialPath = assetPath.Substring(0, pathToFind.Length);
            return partialPath.Equals(pathToFind, StringComparison.OrdinalIgnoreCase);
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

