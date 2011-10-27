using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Cassette.IO;

namespace Cassette.Configuration
{
    public static class BundleCollectionExtensions
    {
        public static void Add<T>(this BundleCollection bundleCollection, string applicationRelativePath)
            where T : Bundle
        {
            Add<T>(bundleCollection, applicationRelativePath, null, null);
        }

        public static void Add<T>(this BundleCollection bundleCollection, string applicationRelativePath, IAssetSource assetSource)
            where T : Bundle
        {
            Add<T>(bundleCollection, applicationRelativePath, assetSource, null);
        }

        public static void Add<T>(this BundleCollection bundleCollection, string applicationRelativePath, Action<T> customizeBundle)
            where T : Bundle
        {
            Add(bundleCollection, applicationRelativePath, null, customizeBundle);
        }

        public static void Add<T>(this BundleCollection bundleCollection, string applicationRelativePath, IAssetSource assetSource, Action<T> customizeBundle)
            where T : Bundle
        {
            Trace.Source.TraceInformation(string.Format("Creating {0} for {1}", typeof(T).Name, applicationRelativePath));
            var bundle = CreateBundle<T>(bundleCollection, applicationRelativePath);

            var source = bundleCollection.Settings.SourceDirectory;
            if (source.DirectoryExists(applicationRelativePath))
            {
                assetSource = assetSource ?? bundleCollection.Settings.DefaultAssetSources[typeof(T)];
                InitializeDirectoryBundle(bundle, source.GetDirectory(applicationRelativePath), assetSource);
            }
            else
            {
                InitializeSingleFileBundle(bundle, source.GetFile(applicationRelativePath));
            }

            if (customizeBundle != null)
            {
                customizeBundle(bundle);
            }

            TraceAssetFilePaths(bundle);

            bundleCollection.Add(bundle);
        }

        static T CreateBundle<T>(BundleCollection bundleCollection, string applicationRelativePath) where T : Bundle
        {
            var bundleFactory = bundleCollection.Settings.BundleFactories[typeof(T)];
            return (T)bundleFactory.CreateBundle(applicationRelativePath, null);
        }

        static void InitializeDirectoryBundle<T>(T bundle, IDirectory directory, IAssetSource assetSource) where T : Bundle
        {
            var assets = assetSource.GetAssets(directory, bundle);
            foreach (var asset in assets)
            {
                bundle.Assets.Add(asset);
            }
        }

        static void InitializeSingleFileBundle<T>(T bundle, IFile file) where T : Bundle
        {
            if (file.Exists)
            {
                bundle.Assets.Add(new Asset(bundle, file));
            }
            else
            {
                throw new DirectoryNotFoundException(string.Format("Bundle path not found: {0}", file.FullPath));
            }
        }

        static void TraceAssetFilePaths<T>(T bundle) where T : Bundle
        {
            foreach (var asset in bundle.Assets)
            {
                Trace.Source.TraceInformation(string.Format("Added asset {0}", asset.SourceFile.FullPath));
            }
        }

        public static void AddForEachSubDirectory<T>(this BundleCollection bundleCollection, string applicationRelativePath)
            where T : Bundle
        {
            AddForEachSubDirectory<T>(bundleCollection, applicationRelativePath, null, null);
        }

        public static void AddForEachSubDirectory<T>(this BundleCollection bundleCollection, string applicationRelativePath, Action<T> customizeBundle)
            where T : Bundle
        {
            AddForEachSubDirectory<T>(bundleCollection, applicationRelativePath, null, customizeBundle);
        }

        public static void AddForEachSubDirectory<T>(this BundleCollection bundleCollection, string applicationRelativePath, IAssetSource initializer)
            where T : Bundle
        {
            AddForEachSubDirectory<T>(bundleCollection, applicationRelativePath, initializer, null);            
        }

        public static void AddForEachSubDirectory<T>(this BundleCollection bundleCollection, string applicationRelativePath, IAssetSource initializer, Action<T> customizeBundle)
            where T : Bundle
        {
            
        }

        static bool IsNotHidden(IDirectory directory)
        {
            return !directory.Attributes.HasFlag(FileAttributes.Hidden);
        }

        static IEnumerable<T> CreateBundles<T>(IBundleFactory<T> bundleFactory, IEnumerable<IDirectory> subDirectories)
            where T : Bundle
        {
            return from subDirectory in subDirectories
                   let bundleDescriptor = GetBundleDescriptor(subDirectory)
                   select bundleFactory.CreateBundle(subDirectory.FullPath, bundleDescriptor);
        }

        static BundleDescriptor GetBundleDescriptor(IDirectory subDirectory)
        {
            var bundleDescriptorFile = subDirectory.GetFile("bundle.txt");

            // TODO: Remove legacy support for module.txt
            if (!bundleDescriptorFile.Exists)
            {
                bundleDescriptorFile = subDirectory.GetFile("module.txt");
            }

            if (bundleDescriptorFile.Exists)
            {
                return new BundleDescriptorReader(bundleDescriptorFile).Read();
            }
            else
            {
                return new BundleDescriptor(new[] { "*" });
            }
        }
    }
}