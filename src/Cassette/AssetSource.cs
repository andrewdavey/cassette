using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using Cassette.IO;

namespace Cassette
{
    public class AssetSource : IAssetSource
    {
        public string FilePattern { get; set; }
        public Regex ExcludeFilePath { get; set; }
        public SearchOption SearchOption { get; set; }

        public IEnumerable<IAsset> GetAssets(IDirectory directory, Bundle bundle)
        {
            return from pattern in GetFilePatterns()
                   from file in directory.GetFiles(pattern, SearchOption)
                   where IsAssetFile(file)
                   select new Asset(bundle, file);
        }

        IEnumerable<string> GetFilePatterns()
        {
            return string.IsNullOrWhiteSpace(FilePattern)
                       ? new[] { "*" }
                       : FilePattern.Split(new[] { ';', ',' }, StringSplitOptions.RemoveEmptyEntries);
        }

        bool IsAssetFile(IFile file)
        {
            return !IsDescriptorFilename(file)
                   && (ExcludeFilePath == null || !ExcludeFilePath.IsMatch(file.FullPath));
        }

        static bool IsDescriptorFilename(IFile file)
        {
            // TODO: Remove legacy support for module.txt
            return file.FullPath.EndsWith("/bundle.txt", StringComparison.OrdinalIgnoreCase)
                   || file.FullPath.EndsWith("/module.txt", StringComparison.OrdinalIgnoreCase);
        }
    }
}