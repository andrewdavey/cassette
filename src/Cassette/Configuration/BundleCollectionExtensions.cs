using System;
using System.Collections.Generic;
using System.Linq;
using Cassette.IO;
using System.IO;

namespace Cassette.Configuration
{
    public static class BundleCollectionExtensions
    {
        public static void AddForEachSubDirectory<T>(this BundleCollection bundleCollection, string path, Action<T> customizeBundle = null)
            where T : Bundle
        {
            var bundleFactory = (IBundleFactory<T>)bundleCollection.Settings.BundleFactories[typeof(T)];
            var directory = bundleCollection.Settings.SourceDirectory.GetDirectory(path);
            var subDirectories = directory.GetDirectories().Where(IsNotHidden);
            var bundles = CreateBundles(bundleFactory, subDirectories);

            foreach (var bundle in bundles)
            {
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