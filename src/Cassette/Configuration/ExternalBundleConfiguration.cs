using System;
using Cassette.IO;

namespace Cassette.Configuration
{
    class ExternalBundleConfiguration<T> : IAddUrlConfiguration
        where T : Bundle
    {
        readonly BundleCollection bundleCollection;
        readonly T bundle;
        readonly string url;
        readonly Action<T> customizeBundle;

        public ExternalBundleConfiguration(BundleCollection bundleCollection, T bundle, string url, Action<T> customizeBundle)
        {
            this.bundleCollection = bundleCollection;
            this.bundle = bundle;
            this.url = url;
            this.customizeBundle = customizeBundle;
        }

        public void WithAlias(string applicationRelativePath)
        {
            if (applicationRelativePath == null) throw new ArgumentNullException("applicationRelativePath");

            bundleCollection.Remove(bundle);
            var factory = GetBundleFactory();
            var newBundle = factory.CreateBundle(
                applicationRelativePath,
                new IFile[0],
                new BundleDescriptor { ExternalUrl = url }
                );
            if (customizeBundle != null) customizeBundle(newBundle);
            bundleCollection.Add(newBundle);
        }

        public void WithDebug(string applicationRelativePath)
        {
            WithDebug(applicationRelativePath, null);
        }

        public void WithDebug(string applicationRelativePath, IFileSource fileSource)
        {
            if (applicationRelativePath == null) throw new ArgumentNullException("applicationRelativePath");

            ReplaceBundleUsingFileSource(
                new BundleDescriptor { ExternalUrl = url, AssetFilenames = { "*" } },
                applicationRelativePath,
                fileSource
                );
        }

        public void WithFallback(string fallbackCondition, string applicationRelativePath)
        {
            WithFallback(fallbackCondition, applicationRelativePath, null);
        }

        public void WithFallback(string fallbackCondition, string applicationRelativePath, IFileSource fileSource)
        {
            if (string.IsNullOrWhiteSpace(fallbackCondition)) throw new ArgumentException("Fallback condition is required.");
            if (applicationRelativePath == null) throw new ArgumentNullException("applicationRelativePath");

            ReplaceBundleUsingFileSource(
                new BundleDescriptor { ExternalUrl = url, FallbackCondition = fallbackCondition, AssetFilenames = { "*" } },
                applicationRelativePath,
                fileSource
                );
        }

        void ReplaceBundleUsingFileSource(BundleDescriptor descriptor, string applicationRelativePath, IFileSource fileSource)
        {
            bundleCollection.Remove(bundle);
            var factory = GetBundleFactory();

            var directory = bundleCollection.Settings.SourceDirectory.GetDirectory(applicationRelativePath);
            fileSource = fileSource ?? GetDefaultFileSource();
            var files = fileSource.GetFiles(directory);
            var newBundle = BundleCollectionExtensions.CreateDirectoryBundle(
                applicationRelativePath,
                factory,
                files,
                directory,
                descriptor
                );
            if (customizeBundle != null) customizeBundle(newBundle);
            bundleCollection.Add(newBundle);
        }

        IFileSource GetDefaultFileSource()
        {
            return bundleCollection.Settings.DefaultFileSources[typeof(T)];
        }

        IBundleFactory<T> GetBundleFactory()
        {
            return (IBundleFactory<T>)bundleCollection.Settings.BundleFactories[typeof(T)];
        }
    }
}