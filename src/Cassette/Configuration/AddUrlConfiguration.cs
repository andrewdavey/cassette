using System;
using System.IO;
using Cassette.IO;

namespace Cassette.Configuration
{
    class AddUrlConfiguration<T> : IAddUrlConfiguration
        where T : Bundle
    {
        readonly BundleCollection bundleCollection;
        readonly T bundle;
        readonly string url;
        readonly Action<T> customizeBundle;

        public AddUrlConfiguration(BundleCollection bundleCollection, T bundle, string url, Action<T> customizeBundle)
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

        public void WithDebug(string applicationRelativePath, IFileSearch fileSearch)
        {
            if (applicationRelativePath == null) throw new ArgumentNullException("applicationRelativePath");

            ReplaceBundleUsingFileSearch(
                new BundleDescriptor { ExternalUrl = url, AssetFilenames = { "*" } },
                applicationRelativePath,
                fileSearch
                );
        }

        public void WithFallback(string fallbackCondition, string applicationRelativePath)
        {
            WithFallback(fallbackCondition, applicationRelativePath, null);
        }

        public void WithFallback(string fallbackCondition, string applicationRelativePath, IFileSearch fileSearch)
        {
            if (string.IsNullOrWhiteSpace(fallbackCondition)) throw new ArgumentException("Fallback condition is required.");
            if (applicationRelativePath == null) throw new ArgumentNullException("applicationRelativePath");

            ReplaceBundleUsingFileSearch(
                new BundleDescriptor { ExternalUrl = url, FallbackCondition = fallbackCondition, AssetFilenames = { "*" } },
                applicationRelativePath,
                fileSearch
                );
        }

        void ReplaceBundleUsingFileSearch(BundleDescriptor descriptor, string applicationRelativePath, IFileSearch fileSearch)
        {
            var factory = GetBundleFactory();
            var sourceDirectory = bundleCollection.Settings.SourceDirectory;

            T newBundle;
            if (sourceDirectory.DirectoryExists(applicationRelativePath))
            {
                var directory = sourceDirectory.GetDirectory(applicationRelativePath);
                fileSearch = fileSearch ?? GetDefaultFileSource();
                var files = fileSearch.FindFiles(directory);
                newBundle = BundleCollectionExtensions.CreateDirectoryBundle(
                    applicationRelativePath,
                    factory,
                    files,
                    directory,
                    descriptor
                );
            }
            else
            {
                var singleFile = sourceDirectory.GetFile(applicationRelativePath);
                if (singleFile.Exists)
                {
                    newBundle = BundleCollectionExtensions.CreateSingleFileBundle(
                        applicationRelativePath,
                        singleFile,
                        factory,
                        descriptor
                    );
                }
                else
                {
                    throw new DirectoryNotFoundException(string.Format("File or directory not found: \"{0}\"", applicationRelativePath));
                }
            }

            bundleCollection.Remove(bundle);
            if (customizeBundle != null) customizeBundle(newBundle);
            bundleCollection.Add(newBundle);
        }

        IFileSearch GetDefaultFileSource()
        {
            return bundleCollection.Settings.DefaultFileSearches[typeof(T)];
        }

        IBundleFactory<T> GetBundleFactory()
        {
            return (IBundleFactory<T>)bundleCollection.Settings.BundleFactories[typeof(T)];
        }
    }
}