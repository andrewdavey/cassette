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
                var allFiles = fileSource.GetFiles(directory);
                bundle = CreateDirectoryBundle(applicationRelativePath, bundleFactory, allFiles, directory);
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

        internal static T CreateDirectoryBundle<T>(string applicationRelativePath, IBundleFactory<T> bundleFactory, IEnumerable<IFile> allFiles,
                                          IDirectory directory, BundleDescriptor descriptor = null) where T : Bundle
        {
            var descriptorFile = TryGetDescriptorFile(directory);
            if (descriptor == null)
            {
                descriptor = descriptorFile.Exists
                    ? new BundleDescriptorReader(descriptorFile).Read()
                    : new BundleDescriptor { AssetFilenames = { "*" } };
            }
            return bundleFactory.CreateBundle(applicationRelativePath, allFiles, descriptor);
        }

        static IFile TryGetDescriptorFile(IDirectory directory)
        {
            var descriptorFile = directory.GetFile("bundle.txt");

            // TODO: Remove this legacy support for module.txt
            if (!descriptorFile.Exists) descriptorFile = directory.GetFile("module.txt");

            return descriptorFile;
        }

        /// <summary>
        /// Adds a bundle for each sub-directory of the given path.
        /// </summary>
        /// <typeparam name="T">The type of bundles to create.</typeparam>
        /// <param name="bundleCollection">The collection to add to.</param>
        /// <param name="applicationRelativePath">The path to the directory containing sub-directories.</param>
        /// <param name="excludeTopLevel">Prevents the creation of an extra bundle from the top-level files of the path, if any.</param>
        public static void AddPerSubDirectory<T>(this BundleCollection bundleCollection, string applicationRelativePath, bool excludeTopLevel = false)
            where T : Bundle
        {
            AddPerSubDirectory<T>(bundleCollection, applicationRelativePath, null, null, excludeTopLevel);
        }

        /// <summary>
        /// Adds a bundle for each sub-directory of the given path.
        /// </summary>
        /// <typeparam name="T">The type of bundles to create.</typeparam>
        /// <param name="bundleCollection">The collection to add to.</param>
        /// <param name="applicationRelativePath">The path to the directory containing sub-directories.</param>
        /// <param name="fileSource">A file source that gets the files to include from a directory.</param>
        /// <param name="excludeTopLevel">Prevents the creation of an extra bundle from the top-level files of the directory, if any.</param>
        public static void AddPerSubDirectory<T>(this BundleCollection bundleCollection, string applicationRelativePath, IFileSource fileSource, bool excludeTopLevel = false)
            where T : Bundle
        {
            AddPerSubDirectory<T>(bundleCollection, applicationRelativePath, fileSource, null, excludeTopLevel);            
        }

        /// <summary>
        /// Adds a bundle for each sub-directory of the given path.
        /// </summary>
        /// <typeparam name="T">The type of bundles to create.</typeparam>
        /// <param name="bundleCollection">The collection to add to.</param>
        /// <param name="applicationRelativePath">The path to the directory containing sub-directories.</param>
        /// <param name="customizeBundle">A delegate that is called for each created bundle to allow customization.</param>
        /// <param name="excludeTopLevel">Prevents the creation of an extra bundle from the top-level files of the path, if any.</param>
        public static void AddPerSubDirectory<T>(this BundleCollection bundleCollection, string applicationRelativePath, Action<T> customizeBundle, bool excludeTopLevel = false)
            where T : Bundle
        {
            AddPerSubDirectory(bundleCollection, applicationRelativePath, null, customizeBundle, excludeTopLevel);
        }

        /// <summary>
        /// Adds a bundle for each sub-directory of the given path.
        /// </summary>
        /// <typeparam name="T">The type of bundles to create.</typeparam>
        /// <param name="bundleCollection">The collection to add to.</param>
        /// <param name="applicationRelativePath">The path to the directory containing sub-directories.</param>
        /// <param name="fileSource">A file source that gets the files to include from a directory.</param>
        /// <param name="customizeBundle">A delegate that is called for each created bundle to allow customization.</param>
        /// <param name="excludeTopLevel">Prevents the creation of an extra bundle from the top-level files of the path, if any.</param>
        public static void AddPerSubDirectory<T>(this BundleCollection bundleCollection, string applicationRelativePath, IFileSource fileSource, Action<T> customizeBundle, bool excludeTopLevel = false)
            where T : Bundle
        {
            Trace.Source.TraceInformation(string.Format("Creating {0} for each subdirectory of {1}", typeof(T).Name, applicationRelativePath));

            fileSource = fileSource ?? bundleCollection.Settings.DefaultFileSources[typeof(T)];

            var bundleFactory = (IBundleFactory<T>)bundleCollection.Settings.BundleFactories[typeof(T)];
            var parentDirectory = bundleCollection.Settings.SourceDirectory.GetDirectory(applicationRelativePath);

            if (!excludeTopLevel)
            {
                var topLevelFiles = fileSource.GetFiles(parentDirectory)
                                              .Where(f => f.Directory == parentDirectory)
                                              .ToArray();
                if (topLevelFiles.Any())
                {
                    var directoryBundle = CreateDirectoryBundle(applicationRelativePath, bundleFactory, topLevelFiles, parentDirectory);
                    bundleCollection.Add(directoryBundle);
                }
            }

            var directories = parentDirectory.GetDirectories().Where(IsNotHidden);
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

        public static IAddUrlConfiguration AddUrl<T>(this BundleCollection bundleCollection, string url, Action<T> customizeBundle = null)
            where T : Bundle
        {
            var bundleFactory = (IBundleFactory<T>)bundleCollection.Settings.BundleFactories[typeof(T)];
            var bundle = bundleFactory.CreateExternalBundle(url);
            if (customizeBundle != null) customizeBundle(bundle);
            bundleCollection.Add(bundle);
            return new ExternalBundleConfiguration<T>(bundleCollection, bundle, url, customizeBundle);
        }

        public static IAddUrlConfiguration AddUrl(this BundleCollection bundleCollection, string url, Action<Bundle> customizeBundle = null)
        {
            if (url.EndsWith(".js"))
            {
                return AddUrl<Scripts.ScriptBundle>(bundleCollection, url, customizeBundle);
            }
            else if (url.EndsWith(".css"))
            {
                return AddUrl<Stylesheets.StylesheetBundle>(bundleCollection, url, customizeBundle);
            }
            else
            {
                throw new ArgumentException("Cannot determine the type of bundle to add. Specify the type using the generic overload of this method.");
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

    public interface IAddUrlConfiguration
    {
        /// <summary>
        /// Defines an application relative path to use as an alias for the bundle.
        /// </summary>
        /// <param name="applicationRelativePath">The application relative path to use as an alias for the bundle.</param>
        void WithAlias(string applicationRelativePath);
        /// <summary>
        /// Configures the bundle to use local assets when in debug mode.
        /// </summary>
        /// <param name="applicationRelativePath">The application relative path to the assets.</param>
        void WithDebug(string applicationRelativePath);
        /// <summary>
        /// Configures the bundle to use local assets when in debug mode.
        /// </summary>
        /// <param name="applicationRelativePath">The application relative path to the assets.</param>
        /// <param name="fileSource">The file source used to get files.</param>
        void WithDebug(string applicationRelativePath, IFileSource fileSource);
        /// <summary>
        /// Configures the bundle to use local assets when the URL fails to load.
        /// </summary>
        /// <param name="fallbackCondition">The JavaScript fallback condition. When true the fallback assets are used.</param>
        /// <param name="applicationRelativePath">The file source used to get files.</param>
        void WithFallback(string fallbackCondition, string applicationRelativePath);
        void WithFallback(string fallbackCondition, string applicationRelativePath, IFileSource fileSource);
    }

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