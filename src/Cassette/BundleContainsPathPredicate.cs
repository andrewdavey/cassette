using System;
using System.IO;
using Cassette.Utilities;

namespace Cassette
{
    class BundleContainsPathPredicate : IBundleVisitor
    {
        public BundleContainsPathPredicate(string path)
        {
            originalPath = path;
        }

        readonly string originalPath;
        string normalizedPath;
        bool isFound;

        public bool AllowPartialAssetPaths { get; set; }

        public bool Result
        {
            get { return isFound; }
        }

        void IBundleVisitor.Visit(Bundle bundle)
        {
            normalizedPath = originalPath.IsUrl() ? originalPath : NormalizePath(originalPath, bundle);

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
            if (assetPath.Length < originalPath.Length) return false;

            var partialPath = assetPath.Substring(0, originalPath.Length);
            return partialPath.Equals(originalPath, StringComparison.OrdinalIgnoreCase);
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
            return PathUtilities.PathsEqual(path, normalizedPath);
        }
    }
}