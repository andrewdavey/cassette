using System;
using System.Collections.Generic;
using System.Linq;

namespace Cassette.HtmlTemplates
{
    public class HtmlTemplateIdBuilder : IHtmlTemplateIdStrategy
    {
        readonly bool includeBundlePath;
        readonly bool includeFileExtension;
        readonly string pathSeparatorReplacement;
        HtmlTemplateBundle currentBundle;

        public HtmlTemplateIdBuilder(bool includeBundlePath = false, bool includeFileExtension = false, string pathSeparatorReplacement = null)
        {
            this.includeBundlePath = includeBundlePath;
            this.includeFileExtension = includeFileExtension;
            this.pathSeparatorReplacement = pathSeparatorReplacement;
        }

        public string HtmlTemplateId(HtmlTemplateBundle bundle, IAsset htmlTemplateAsset)
        {
            currentBundle = bundle;
            var operations = BuildOperations();
            return ApplyOperations(htmlTemplateAsset, operations);
        }

        IEnumerable<Func<string, string>> BuildOperations()
        {
            var operations = new List<Func<string, string>>();
            if (!includeBundlePath)
            {
                operations.Add(RemoveBundlePath);
            }
            if (!includeFileExtension)
            {
                operations.Add(RemoveFileExtension);
            }
            operations.Add(RemoveTildeSlash);
            if (pathSeparatorReplacement != null)
            {
                operations.Add(ReplacePathSeparators);
            }
            return operations;
        }

        string ApplyOperations(IAsset htmlTemplateAsset, IEnumerable<Func<string, string>> operations)
        {
            return operations.Aggregate(htmlTemplateAsset.Path, (path, operation) => operation(path));
        }

        string RemoveBundlePath(string assetPath)
        {
            var bundlePath = currentBundle.Path;
            if (assetPath.StartsWith(bundlePath, StringComparison.OrdinalIgnoreCase))
            {
                return assetPath.Substring(bundlePath.Length + 1);
            }
            else
            {
                return assetPath;
            }
        }

        string RemoveTildeSlash(string path)
        {
            return path.TrimStart('~', '/');
        }

        string RemoveFileExtension(string path)
        {
            var index = path.LastIndexOf('.');
            var lastSlash = path.LastIndexOf('/');
            var hasExtension = index >= 0 && index >= lastSlash;
            return hasExtension ? path.Substring(0, index) : path;
        }

        string ReplacePathSeparators(string path)
        {
            return path.Replace("/", pathSeparatorReplacement);
        }
    }
}