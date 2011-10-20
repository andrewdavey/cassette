using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using Cassette.IO;

namespace Cassette
{
    class BundleDirectoryInitializer : IBundleInitializer
    {
        public BundleDirectoryInitializer(string path)
        {
            Path = path;
        }

        public string Path { get; set; }
        public string FilePattern { get; set; }
        public Regex ExcludeFilePath { get; set; }
        public SearchOption SearchOption { get; set; }
        public BundleDescriptor BundleDescriptor { get; set; }

        public void InitializeBundle(Bundle bundle, ICassetteApplication application)
        {
            var directory = application.SourceDirectory.GetDirectory(Path, false);
            var descriptor = GetOrLoadBundleDescriptor(directory);
            IEnumerable<IAsset> assets;
            if (descriptor == null)
            {
                assets = GetAllAssets(bundle, directory);
            }
            else
            {
                bundle.AddReferences(descriptor.References);

                var files = descriptor.GetAssetFiles(directory, GetFilePatterns(), ExcludeFilePath, SearchOption);
                assets = files.Select(file => new Asset(bundle, file));
            }
            AddAssetsToBundle(bundle, assets);
        }

        BundleDescriptor GetOrLoadBundleDescriptor(IDirectory directory)
        {
            if (BundleDescriptor != null) return BundleDescriptor;

            var file = directory.GetFile("bundle.txt");
            // TODO: Remove legacy support for module.txt
            if (!file.Exists) file = directory.GetFile("module.txt");

            if (file.Exists)
            {
                return new BundleDescriptorReader(file).Read();
            }
            else
            {
                return null;
            }
        }

        IEnumerable<Asset> GetAllAssets(Bundle bundle, IDirectory directory)
        {
            return from pattern in GetFilePatterns()
                   from file in directory.GetFiles(pattern, SearchOption)
                   where !IsDescriptorFilename(file)
                         && (ExcludeFilePath == null || !ExcludeFilePath.IsMatch(file.FullPath))
                   select new Asset(bundle, file);
        }

        void AddAssetsToBundle(Bundle bundle, IEnumerable<IAsset> assets)
        {
            foreach (var asset in assets)
            {
                bundle.Assets.Add(asset);
            }
        }

        static bool IsDescriptorFilename(IFile file)
        {
            // TODO: Remove legacy support for module.txt
            return file.FullPath.EndsWith("bundle.txt", StringComparison.OrdinalIgnoreCase)
                   || file.FullPath.EndsWith("module.txt", StringComparison.OrdinalIgnoreCase);
        }

        IEnumerable<string> GetFilePatterns()
        {
            return string.IsNullOrWhiteSpace(FilePattern)
                       ? new[] { "*" }
                       : FilePattern.Split(new[] { ';', ',' }, StringSplitOptions.RemoveEmptyEntries);
        }
    }
}