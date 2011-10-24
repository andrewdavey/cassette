using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Cassette.IO;

namespace Cassette.Configuration
{
    public static class BundleCollectionExtensions
    {
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

        public static void AddForEachSubDirectory<T>(this BundleCollection bundleCollection, string applicationRelativePath, IBundleInitializer initializer)
            where T : Bundle
        {
            AddForEachSubDirectory<T>(bundleCollection, applicationRelativePath, initializer, null);            
        }

        public static void AddForEachSubDirectory<T>(this BundleCollection bundleCollection, string applicationRelativePath, IBundleInitializer initializer, Action<T> customizeBundle)
            where T : Bundle
        {
            var bundleFactory = (IBundleFactory<T>)bundleCollection.Settings.BundleFactories[typeof(T)];
            var directory = bundleCollection.Settings.SourceDirectory.GetDirectory(applicationRelativePath);
            var subDirectories = directory.GetDirectories().Where(IsNotHidden);
            var bundles = CreateBundles(bundleFactory, subDirectories);

            foreach (var bundle in bundles)
            {
                if (initializer != null) bundle.BundleInitializers.Add(initializer);
                if (customizeBundle != null) customizeBundle(bundle);
                bundleCollection.Add(bundle);
            }
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