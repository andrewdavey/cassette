using System;
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

        public static void Add<T>(this BundleCollection bundleCollection, string applicationRelativePath, IFileSource fileSource)
            where T : Bundle
        {
            Add<T>(bundleCollection, applicationRelativePath, fileSource, null);
        }

        public static void Add<T>(this BundleCollection bundleCollection, string applicationRelativePath, Action<T> customizeBundle)
            where T : Bundle
        {
            Add(bundleCollection, applicationRelativePath, null, customizeBundle);
        }

        public static void Add<T>(this BundleCollection bundleCollection, string applicationRelativePath, IFileSource fileSource, Action<T> customizeBundle)
            where T : Bundle
        {
            Trace.Source.TraceInformation(string.Format("Creating {0} for {1}", typeof(T).Name, applicationRelativePath));

            T bundle;
            var bundleFactory = (IBundleFactory<T>)bundleCollection.Settings.BundleFactories[typeof(T)];
            
            var source = bundleCollection.Settings.SourceDirectory;
            if (source.DirectoryExists(applicationRelativePath))
            {
                fileSource = fileSource ?? bundleCollection.Settings.DefaultFileSources[typeof(T)];
                var directory = source.GetDirectory(applicationRelativePath);
                var descriptorFile = TryGetDescriptorFile(directory);
                var descriptor = descriptorFile.Exists 
                    ? new BundleDescriptorReader(descriptorFile).Read() 
                    : new BundleDescriptor { AssetFilenames = { "*" } };
                var allFiles = fileSource.GetFiles(directory);
                bundle = bundleFactory.CreateBundle(applicationRelativePath, allFiles, descriptor);
            }
            else
            {
                var file = source.GetFile(applicationRelativePath);
                if (file.Exists)
                {
                    var descriptor = new BundleDescriptor
                    {
                        AssetFilenames = { applicationRelativePath }
                    };
                    bundle = bundleFactory.CreateBundle(applicationRelativePath, new[] { file }, descriptor);
                }
                else
                {
                    throw new DirectoryNotFoundException(string.Format("Bundle path not found: {0}", applicationRelativePath));
                }
            }

            if (customizeBundle != null)
            {
                customizeBundle(bundle);
            }

            TraceAssetFilePaths(bundle);

            bundleCollection.Add(bundle);
        }

        static IFile TryGetDescriptorFile(IDirectory directory)
        {
            var descriptorFile = directory.GetFile("bundle.txt");

            // TODO: Remove this legacy support for module.txt
            if (!descriptorFile.Exists) descriptorFile = directory.GetFile("module.txt");

            return descriptorFile;
        }

        public static void AddPerSubDirectory<T>(this BundleCollection bundleCollection, string applicationRelativePath)
            where T : Bundle
        {
            AddPerSubDirectory<T>(bundleCollection, applicationRelativePath, null, null);
        }

        public static void AddPerSubDirectory<T>(this BundleCollection bundleCollection, string applicationRelativePath, IFileSource assetSource)
            where T : Bundle
        {
            AddPerSubDirectory<T>(bundleCollection, applicationRelativePath, assetSource, null);            
        }

        public static void AddPerSubDirectory<T>(this BundleCollection bundleCollection, string applicationRelativePath, Action<T> customizeBundle)
            where T : Bundle
        {
            AddPerSubDirectory(bundleCollection, applicationRelativePath, null, customizeBundle);
        }

        public static void AddPerSubDirectory<T>(this BundleCollection bundleCollection, string applicationRelativePath, IFileSource fileSource, Action<T> customizeBundle)
            where T : Bundle
        {
            Trace.Source.TraceInformation(string.Format("Creating {0} for each subdirectory of {1}", typeof(T).Name, applicationRelativePath));

            var bundleFactory = (IBundleFactory<T>)bundleCollection.Settings.BundleFactories[typeof(T)];
            var parentDirectory = bundleCollection.Settings.SourceDirectory.GetDirectory(applicationRelativePath);
            var directories = parentDirectory.GetDirectories().Where(IsNotHidden);
            fileSource = fileSource ?? bundleCollection.Settings.DefaultFileSources[typeof(T)];
            foreach (var directory in directories)
            {
                Trace.Source.TraceInformation(string.Format("Creating {0} for {1}", typeof(T).Name, applicationRelativePath));
                var descriptorFile = TryGetDescriptorFile(directory);
                var descriptor = descriptorFile.Exists
                    ? new BundleDescriptorReader(descriptorFile).Read()
                    : new BundleDescriptor { AssetFilenames = { "*" } };
                var allFiles = fileSource.GetFiles(directory);
                var bundle = bundleFactory.CreateBundle(directory.FullPath, allFiles, descriptor);
                if (customizeBundle != null) customizeBundle(bundle);
                TraceAssetFilePaths(bundle);
                bundleCollection.Add(bundle);
            }
        }

        static void TraceAssetFilePaths<T>(T bundle) where T : Bundle
        {
            foreach (var asset in bundle.Assets)
            {
                Trace.Source.TraceInformation(string.Format("Added asset {0}", asset.SourceFile.FullPath));
            }
        }

        static bool IsNotHidden(IDirectory directory)
        {
            return !directory.Attributes.HasFlag(FileAttributes.Hidden);
        }
    }
}