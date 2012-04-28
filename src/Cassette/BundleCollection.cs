using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using Cassette.IO;
using Cassette.Utilities;
#if NET35
using Iesi.Collections.Generic;
#endif

namespace Cassette
{
    public class BundleCollection : IEnumerable<Bundle>, IDisposable
    {
        readonly List<Bundle> bundles = new List<Bundle>();
        readonly CassetteSettings settings;
        readonly IFileSearchProvider fileSearchProvider;
        readonly IBundleFactoryProvider bundleFactoryProvider;
        readonly ReaderWriterLockSlim readerWriterLock = new ReaderWriterLockSlim();
        Dictionary<Bundle, HashedSet<Bundle>> bundleImmediateReferences;
        Exception initializationException;
        
        public BundleCollection(CassetteSettings settings, IFileSearchProvider fileSearchProvider, IBundleFactoryProvider bundleFactoryProvider)
        {
            this.settings = settings;
            this.fileSearchProvider = fileSearchProvider;
            this.bundleFactoryProvider = bundleFactoryProvider;
        }

        /// <summary>
        /// An exception occuring during bundle collection initialization can be stored here.
        /// Then during GetReadLock it is thrown.
        /// </summary>
        internal Exception InitializationException
        {
            get
            {
                return initializationException;
            }
            set
            {
                using (GetWriteLock())
                {
                    initializationException = value;
                }
            }
        }

        public IDisposable GetReadLock()
        {
            readerWriterLock.EnterReadLock();

            if (InitializationException != null)
            {
                readerWriterLock.ExitReadLock();
                throw new Exception("Bundle collection rebuild failed. See inner exception for details.", InitializationException);
            }

            return new DelegatingDisposable(() => readerWriterLock.ExitReadLock());
        }

        public IDisposable GetWriteLock()
        {
            readerWriterLock.EnterWriteLock();
            return new DelegatingDisposable(() => readerWriterLock.ExitWriteLock());
        }

        public void Accept(IBundleVisitor bundleVisitor)
        {
            foreach (var bundle in bundles)
            {
                bundle.Accept(bundleVisitor);
            }
        }

        /// <summary>
        /// Adds a <see cref="Bundle"/> to the collection.
        /// </summary>
        /// <param name="bundle">The bundle to add.</param>
        public void Add(Bundle bundle)
        {
            var bundleTypeAlreadyAdded = bundles.Any(
                b => b.ContainsPath(bundle.Path)
                  && b.GetType() == bundle.GetType()
            );
            if (bundleTypeAlreadyAdded)
            {
                throw new ArgumentException(
                    string.Format("A {0} with the path \"{1}\" has already been added to the collection.", bundle.GetType().Name, bundle.Path)
                );
            }
            bundles.Add(bundle);
        }

        public void AddRange(IEnumerable<Bundle> bundles)
        {
            foreach (var bundle in bundles)
            {
                Add(bundle);
            }
        }

        internal void Process()
        {
            foreach (var bundle in bundles)
            {
                bundle.Process(settings);
            }
        }

        void Remove(Bundle bundle)
        {
            bundles.Remove(bundle);
        }

        /// <summary>
        /// Adds a bundle of type <typeparamref name="T"/> using asset files found in the given path.
        /// </summary>
        /// <typeparam name="T">The type of bundle to create.</typeparam>
        /// <param name="applicationRelativePath">The application relative path to the bundle's asset files.</param>
        public void Add<T>(string applicationRelativePath)
            where T : Bundle
        {
            Add<T>(applicationRelativePath, (IFileSearch)null, null);
        }

        /// <summary>
        /// Adds a bundle of type <typeparamref name="T"/> using asset files found in the given path.
        /// </summary>
        /// <typeparam name="T">The type of bundle to create.</typeparam>
        /// <param name="applicationRelativePath">The application relative path to the bundle's asset files.</param>
        /// <param name="fileSearch">The file search used to find asset files to include in the bundle.</param>
        public void Add<T>(string applicationRelativePath, IFileSearch fileSearch)
            where T : Bundle
        {
            Add<T>(applicationRelativePath, fileSearch, null);
        }

        /// <summary>
        /// Adds a bundle of type <typeparamref name="T"/> using asset files found in the given path.
        /// </summary>
        /// <typeparam name="T">The type of bundle to create.</typeparam>
        /// <param name="applicationRelativePath">The application relative path to the bundle's asset files.</param>
        /// <param name="customizeBundle">The delegate used to customize the created bundle before adding it to the collection.</param>
        public void Add<T>(string applicationRelativePath, Action<T> customizeBundle)
            where T : Bundle
        {
            Add(applicationRelativePath, (IFileSearch)null, customizeBundle);
        }

        /// <summary>
        /// Adds a bundle of type <typeparamref name="T"/> using asset files found in the given path.
        /// </summary>
        /// <typeparam name="T">The type of bundle to create.</typeparam>
        /// <param name="applicationRelativePath">The application relative path to the bundle's asset files.</param>
        /// <param name="fileSearch">The file search used to find asset files to include in the bundle.</param>
        /// <param name="customizeBundle">The delegate used to customize the created bundle before adding it to the collection.</param>
        public void Add<T>(string applicationRelativePath, IFileSearch fileSearch, Action<T> customizeBundle)
            where T : Bundle
        {
            applicationRelativePath = PathUtilities.AppRelative(applicationRelativePath);
            Trace.Source.TraceInformation(string.Format("Creating {0} for {1}", typeof(T).Name, applicationRelativePath));

            T bundle;
            var bundleFactory = bundleFactoryProvider.GetBundleFactory<T>();

            var source = settings.SourceDirectory;
            if (source.DirectoryExists(applicationRelativePath))
            {
                fileSearch = fileSearch ?? fileSearchProvider.GetFileSearch(typeof(T));
                var directory = source.GetDirectory(applicationRelativePath);
                var allFiles = fileSearch.FindFiles(directory);
                bundle = CreateDirectoryBundle(applicationRelativePath, bundleFactory, allFiles, directory);
            }
            else
            {
                var file = source.GetFile(applicationRelativePath);
                if (file.Exists)
                {
                    bundle = CreateSingleFileBundle(applicationRelativePath, file, bundleFactory);
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

            Add(bundle);
        }

        /// <summary>
        /// Adds a new bundle with an explicit list of assets.
        /// </summary>
        /// <typeparam name="T">The type of bundle to add.</typeparam>
        /// <param name="applicationRelativePath">The application relative path of the bundle. This does not have to be a real directory path.</param>
        /// <param name="assetFilenames">The filenames of assets to add to the bundle. The order given here will be preserved. Filenames are bundle directory relative, if the bundle path exists, otherwise they are application relative.</param>
        public void Add<T>(string applicationRelativePath, IEnumerable<string> assetFilenames)
            where T : Bundle
        {
            Add<T>(applicationRelativePath, assetFilenames, null);
        }


        /// <summary>
        /// Adds a new bundle with an explicit list of assets.
        /// </summary>
        /// <typeparam name="T">The type of bundle to add.</typeparam>
        /// <param name="applicationRelativePath">The application relative path of the bundle. This does not have to be a real directory path.</param>
        /// <param name="assetFilenames">The filenames of assets to add to the bundle. The order given here will be preserved. Filenames are bundle directory relative, if the bundle path exists, otherwise they are application relative.</param>
        public void Add<T>(string applicationRelativePath, params string[] assetFilenames)
            where T : Bundle
        {
            Add<T>(applicationRelativePath, assetFilenames, null);
        }

        /// <summary>
        /// Adds a new bundle with an explicit list of assets.
        /// </summary>
        /// <typeparam name="T">The type of bundle to add.</typeparam>
        /// <param name="applicationRelativePath">The application relative path of the bundle. This does not have to be a real directory path.</param>
        /// <param name="assetFilenames">The filenames of assets to add to the bundle. The order given here will be preserved. Filenames are bundle directory relative, if the bundle path exists, otherwise they are application relative.</param>
        /// <param name="customizeBundle">An action delegate used to customize the created bundle.</param>
        public void Add<T>(string applicationRelativePath, IEnumerable<string> assetFilenames, Action<T> customizeBundle)
            where T : Bundle
        {
            var bundleDirectoryExists = settings.SourceDirectory.DirectoryExists(applicationRelativePath);
            var directory = bundleDirectoryExists
                                ? settings.SourceDirectory.GetDirectory(applicationRelativePath)
                                : settings.SourceDirectory;
            var files = assetFilenames.Select(directory.GetFile);

            var factory = bundleFactoryProvider.GetBundleFactory<T>();

            var bundle = factory.CreateBundle(
                applicationRelativePath,
                files,
                new BundleDescriptor { AssetFilenames = { "*" } }
                );

            bundle.IsSorted = true;

            if (customizeBundle != null)
            {
                customizeBundle(bundle);
            }

            Add(bundle);
        }

        T CreateSingleFileBundle<T>(
            string applicationRelativePath,
            IFile file,
            IBundleFactory<T> bundleFactory,
            BundleDescriptor descriptor = null) where T : Bundle
        {
            descriptor = descriptor ?? new BundleDescriptor
            {
                AssetFilenames = { applicationRelativePath }
            };
            return bundleFactory.CreateBundle(applicationRelativePath, new[] { file }, descriptor);
        }

        T CreateDirectoryBundle<T>(
            string applicationRelativePath,
            IBundleFactory<T> bundleFactory,
            IEnumerable<IFile> allFiles,
            IDirectory directory,
            BundleDescriptor descriptor = null) where T : Bundle
        {
            if (descriptor == null)
            {
                descriptor = ReadOrCreateBundleDescriptor<T>(directory);
            }
            return bundleFactory.CreateBundle(applicationRelativePath, allFiles, descriptor);
        }

        BundleDescriptor ReadOrCreateBundleDescriptor<T>(IDirectory directory)
            where T : Bundle
        {
            var descriptorFile = TryGetDescriptorFile<T>(directory);
            return descriptorFile.Exists
                ? new BundleDescriptorReader(descriptorFile).Read()
                : new BundleDescriptor { AssetFilenames = { "*" } };
        }

        IFile TryGetDescriptorFile<T>(IDirectory directory)
            where T : Bundle
        {
            var typeSpecificDescriptorFilename = typeof(T).Name + ".txt";
            var descriptorFile = directory.GetFile(typeSpecificDescriptorFilename);

            if (!descriptorFile.Exists) descriptorFile = directory.GetFile("bundle.txt");

            // TODO: Remove this legacy support for module.txt
            if (!descriptorFile.Exists) descriptorFile = directory.GetFile("module.txt");

            return descriptorFile;
        }

        /// <summary>
        /// Adds a bundle for each sub-directory of the given path.
        /// </summary>
        /// <typeparam name="T">The type of bundles to create.</typeparam>
        /// <param name="applicationRelativePath">The path to the directory containing sub-directories.</param>
        /// <param name="excludeTopLevel">Prevents the creation of an extra bundle from the top-level files of the path, if any.</param>
        public void AddPerSubDirectory<T>(string applicationRelativePath, bool excludeTopLevel = false)
            where T : Bundle
        {
            AddPerSubDirectory<T>(applicationRelativePath, null, null, excludeTopLevel);
        }

        /// <summary>
        /// Adds a bundle for each sub-directory of the given path.
        /// </summary>
        /// <typeparam name="T">The type of bundles to create.</typeparam>
        /// <param name="applicationRelativePath">The path to the directory containing sub-directories.</param>
        /// <param name="fileSearch">A file source that gets the files to include from a directory.</param>
        /// <param name="excludeTopLevel">Prevents the creation of an extra bundle from the top-level files of the directory, if any.</param>
        public void AddPerSubDirectory<T>(string applicationRelativePath, IFileSearch fileSearch, bool excludeTopLevel = false)
            where T : Bundle
        {
            AddPerSubDirectory<T>(applicationRelativePath, fileSearch, null, excludeTopLevel);
        }

        /// <summary>
        /// Adds a bundle for each sub-directory of the given path.
        /// </summary>
        /// <typeparam name="T">The type of bundles to create.</typeparam>
        /// <param name="applicationRelativePath">The path to the directory containing sub-directories.</param>
        /// <param name="customizeBundle">A delegate that is called for each created bundle to allow customization.</param>
        /// <param name="excludeTopLevel">Prevents the creation of an extra bundle from the top-level files of the path, if any.</param>
        public void AddPerSubDirectory<T>(string applicationRelativePath, Action<T> customizeBundle, bool excludeTopLevel = false)
            where T : Bundle
        {
            AddPerSubDirectory(applicationRelativePath, null, customizeBundle, excludeTopLevel);
        }

        /// <summary>
        /// Adds a bundle for each sub-directory of the given path.
        /// </summary>
        /// <typeparam name="T">The type of bundles to create.</typeparam>
        /// <param name="applicationRelativePath">The path to the directory containing sub-directories.</param>
        /// <param name="fileSearch">A file source that gets the files to include from a directory.</param>
        /// <param name="customizeBundle">A delegate that is called for each created bundle to allow customization.</param>
        /// <param name="excludeTopLevel">Prevents the creation of an extra bundle from the top-level files of the path, if any.</param>
        public void AddPerSubDirectory<T>(string applicationRelativePath, IFileSearch fileSearch, Action<T> customizeBundle, bool excludeTopLevel = false)
            where T : Bundle
        {
            Trace.Source.TraceInformation(string.Format("Creating {0} for each subdirectory of {1}", typeof(T).Name, applicationRelativePath));

            fileSearch = fileSearch ?? fileSearchProvider.GetFileSearch(typeof(T));

            var bundleFactory = bundleFactoryProvider.GetBundleFactory<T>();
            var parentDirectory = settings.SourceDirectory.GetDirectory(applicationRelativePath);

            if (!excludeTopLevel)
            {
                var topLevelFiles = fileSearch.FindFiles(parentDirectory)
                    .Where(f => f.Directory.Equals(parentDirectory))
                    .ToArray();
                var directoryBundle = CreateDirectoryBundle(applicationRelativePath, bundleFactory, topLevelFiles, parentDirectory);
                if (topLevelFiles.Any() || directoryBundle is IExternalBundle)
                {
                    if (customizeBundle != null) customizeBundle(directoryBundle);
                    Add(directoryBundle);
                }
            }

            var directories = parentDirectory.GetDirectories().Where(IsNotHidden);
            foreach (var directory in directories)
            {
                Trace.Source.TraceInformation(string.Format("Creating {0} for {1}", typeof(T).Name, directory.FullPath));
                var allFiles = fileSearch.FindFiles(directory).ToArray();

                var descriptor = ReadOrCreateBundleDescriptor<T>(directory);

                if (!allFiles.Any() && descriptor.ExternalUrl == null) continue;

                var bundle = bundleFactory.CreateBundle(directory.FullPath, allFiles, descriptor);
                if (customizeBundle != null) customizeBundle(bundle);
                TraceAssetFilePaths(bundle);
                Add(bundle);
            }
        }

        public void AddUrlWithLocalAssets(string url, LocalAssetSettings localAssetSettings, Action<Bundle> customizeBundle = null)
        {
            if (url.EndsWith(".js", StringComparison.OrdinalIgnoreCase))
            {
                AddUrlWithLocalAssets<Scripts.ScriptBundle>(url, localAssetSettings, customizeBundle);
            }
            else if (url.EndsWith(".css", StringComparison.OrdinalIgnoreCase))
            {
                AddUrlWithLocalAssets<Stylesheets.StylesheetBundle>(url, localAssetSettings, customizeBundle);
            }
            else
            {
                throw new ArgumentException("Cannot determine the type of bundle to add. Specify the type using the generic overload of this method.");
            }
        }

        public void AddUrlWithLocalAssets<T>(string url, LocalAssetSettings localAssetSettings, Action<Bundle> customizeBundle = null)
            where T : Bundle
        {
            var existingBundle = bundles.FirstOrDefault(
                b => b.ContainsPath(PathUtilities.AppRelative(localAssetSettings.Path))
            );
            if (existingBundle != null)
            {
                Remove(existingBundle);
            }

            var bundleFactory = bundleFactoryProvider.GetBundleFactory<T>();
            var sourceDirectory = settings.SourceDirectory;
            IEnumerable<IFile> files;
            BundleDescriptor bundleDescriptor;

            if (sourceDirectory.DirectoryExists(localAssetSettings.Path))
            {
                var fileSearch = localAssetSettings.FileSearch ?? fileSearchProvider.GetFileSearch(typeof(T));
                var directory = sourceDirectory.GetDirectory(localAssetSettings.Path);
                files = fileSearch.FindFiles(directory);

                bundleDescriptor = ReadOrCreateBundleDescriptor<T>(directory);
            }
            else
            {
                var singleFile = sourceDirectory.GetFile(localAssetSettings.Path);
                if (singleFile.Exists)
                {
                    files = new[] { singleFile };
                    bundleDescriptor = new BundleDescriptor { AssetFilenames = { "*" } };
                }
                else
                {
                    throw new DirectoryNotFoundException(string.Format("File or directory not found: \"{0}\"", localAssetSettings.Path));
                }
            }

            bundleDescriptor.FallbackCondition = localAssetSettings.FallbackCondition;
            bundleDescriptor.ExternalUrl = url;
            var bundle = bundleFactory.CreateBundle(localAssetSettings.Path, files, bundleDescriptor);
            if (customizeBundle != null) customizeBundle(bundle);
            Add(bundle);
        }

        public void AddUrlWithAlias(string url, string alias, Action<Bundle> customizeBundle = null)
        {
            if (url.EndsWith(".js", StringComparison.OrdinalIgnoreCase))
            {
                AddUrlWithAlias<Scripts.ScriptBundle>(url, alias, customizeBundle);
            }
            else if (url.EndsWith(".css", StringComparison.OrdinalIgnoreCase))
            {
                AddUrlWithAlias<Stylesheets.StylesheetBundle>(url, alias, customizeBundle);
            }
            else
            {
                throw new ArgumentException("Cannot determine the type of bundle to add. Specify the type using the generic overload of this method.");
            }
        }

        public void AddUrlWithAlias<T>(string url, string alias, Action<Bundle> customizeBundle = null)
            where T : Bundle
        {
            var bundleFactory = bundleFactoryProvider.GetBundleFactory<T>();
            var bundle = bundleFactory.CreateBundle(
                alias,
                new IFile[0],
                new BundleDescriptor { ExternalUrl = url }
                );
            if (customizeBundle != null) customizeBundle(bundle);
            Add(bundle);
        }

        /// <summary>
        /// Adds a bundle that references a URL instead of local asset files.
        /// </summary>
        /// <typeparam name="T">The type of bundle to create.</typeparam>
        /// <param name="url">The URL to reference.</param>
        /// <param name="customizeBundle">A delegate that is called for each created bundle to allow customization.</param>
        /// <returns>A object used to further configure the bundle.</returns>
        public void AddUrl<T>(string url, Action<Bundle> customizeBundle = null)
            where T : Bundle
        {
            var bundleFactory = bundleFactoryProvider.GetBundleFactory<T>();
            var bundle = bundleFactory.CreateExternalBundle(url);
            if (customizeBundle != null) customizeBundle(bundle);
            Add(bundle);
        }

        /// <summary>
        /// Adds a bundle that references a URL instead of local asset files. The type of bundle created is determined by the URL's file extension.
        /// </summary>
        /// <param name="url">The URL to reference.</param>
        /// <param name="customizeBundle">A delegate that is called for each created bundle to allow customization.</param>
        /// <returns>A object used to further configure the bundle.</returns>
        public void AddUrl(string url, Action<Bundle> customizeBundle = null)
        {
            if (url.EndsWith(".js", StringComparison.OrdinalIgnoreCase))
            {
                AddUrl<Scripts.ScriptBundle>(url, customizeBundle);
            }
            else if (url.EndsWith(".css", StringComparison.OrdinalIgnoreCase))
            {
                AddUrl<Stylesheets.StylesheetBundle>(url, customizeBundle);
            }
            else
            {
                throw new ArgumentException("Cannot determine the type of bundle to add. Specify the type using the generic overload of this method.");
            }
        }

        /// <summary>
        /// Adds a bundle for each individual file found using the file search. If no file search is provided the application
        /// default file search for the bundle type is used.
        /// </summary>
        /// <typeparam name="T">The type of bundle to create.</typeparam>
        /// <param name="directoryPath">The path to the directory to search. If null or empty the application source directory is used.</param>
        /// <param name="fileSearch">The <see cref="IFileSearch"/> used to find files. If null the application default file search for the bundle type is used.</param>
        /// <param name="customizeBundle">An optional action delegate called for each bundle.</param>
        public void AddPerIndividualFile<T>(string directoryPath = null, IFileSearch fileSearch = null, Action<T> customizeBundle = null)
            where T : Bundle
        {
            var directory = string.IsNullOrEmpty(directoryPath)
                                ? settings.SourceDirectory
                                : settings.SourceDirectory.GetDirectory(directoryPath);

            fileSearch = fileSearch ?? fileSearchProvider.GetFileSearch(typeof(T));
            var files = fileSearch.FindFiles(directory);
            var bundleFactory = bundleFactoryProvider.GetBundleFactory<T>();
            foreach (var file in files)
            {
                var bundle = bundleFactory.CreateBundle(
                    file.FullPath,
                    new[] { file },
                    new BundleDescriptor { AssetFilenames = { "*" } }
                    );
                if (customizeBundle != null) customizeBundle(bundle);
                Add(bundle);
            }
        }

        /// <summary>
        /// Gets a strongly-typed <see cref="Bundle"/> from the collection, by path.
        /// </summary>
        /// <typeparam name="T">The type of bundle.</typeparam>
        /// <param name="path">The bundle path to find.</param>
        /// <returns>A strongly-typed bundle</returns>
        /// <exception cref="ArgumentException">Thrown when bundle is not found.</exception>
        public T Get<T>(string path)
            where T : Bundle
        {
            return Get(path, bundleArray => bundleArray.OfType<T>().First());
        }

        /// <summary>
        /// Gets a <see cref="Bundle"/> from the collection, by path.
        /// </summary>
        /// <param name="path">The bundle path to find.</param>
        /// <returns>A bundle.</returns>
        /// <exception cref="ArgumentException">Thrown when bundle is not found.</exception>
        public Bundle Get(string path)
        {
            return Get(path, bundleArray => bundleArray[0]);
        }

        /// <summary>
        /// Gets a <see cref="Bundle"/> from the collection, by path.
        /// </summary>
        /// <returns>A bundle.</returns>
        /// <exception cref="ArgumentException">Thrown when bundle is not found.</exception>
        public Bundle this[string path]
        {
            get { return Get(path); }
        }

        T Get<T>(string path, Func<Bundle[], T> getFromMatching)
        {
            path = PathUtilities.AppRelative(path);
            var matchingBundles = bundles.Where(b => b.ContainsPath(path)).ToArray();

            if (matchingBundles.Length == 0)
            {
                throw new ArgumentException(
                    string.Format("Bundle not found with path \"{0}\".", path)
                );
            }

            return getFromMatching(matchingBundles);
        }

        public bool TryGetAssetByPath(string path, out IAsset asset, out Bundle bundle)
        {
            var results =
                from b in bundles
                where b.ContainsPath(path)
                let a = b.FindAssetByPath(path)
                where a != null
                select new { Bundle = b, Asset = a };

            var result = results.FirstOrDefault();
            if (result != null)
            {
                asset = result.Asset;
                bundle = result.Bundle;
                return true;
            }
            else
            {
                asset = null;
                bundle = null;
                return false;
            }
        }

        void TraceAssetFilePaths<T>(T bundle) where T : Bundle
        {
            foreach (var asset in bundle.Assets)
            {
                Trace.Source.TraceInformation(string.Format("Added asset {0}", asset.Path));
            }
        }

        bool IsNotHidden(IDirectory directory)
        {
            return !directory.Attributes.HasFlag(FileAttributes.Hidden);
        }

        public IEnumerable<Bundle> FindBundlesContainingPath(string path)
        {
            path = PathUtilities.AppRelative(path);
            return bundles.Where(bundle => bundle.ContainsPath(path));
        }

        internal void BuildReferences()
        {
            ValidateBundleReferences();
            ValidateAssetReferences();
            bundleImmediateReferences = BuildBundleImmediateReferenceDictionary();

            var graph = new Graph<Bundle>(bundles, b => bundleImmediateReferences[b]);
            ThrowIfCyclesInBundleGraph(graph);
        }

        void ValidateBundleReferences()
        {
            var notFound = from bundle in bundles
                           from reference in bundle.References
                           where bundles.Any(m => m.ContainsPath(reference)) == false
                           select string.Format(
                               "Reference error in bundle descriptor for \"{0}\". Cannot find \"{1}\".",
                               bundle.Path,
                               reference
                           );
            var message = string.Join(Environment.NewLine, notFound.ToArray());
            if (message.Length > 0)
            {
                throw new AssetReferenceException(message);
            }
        }

        void ValidateAssetReferences()
        {
            var collector = new BundleReferenceCollector(AssetReferenceType.DifferentBundle);
            foreach (var bundle in bundles)
            {
                bundle.Accept(collector);
            }
            var notFound = from reference in collector.CollectedReferences
                           where !reference.SourceBundle.IsFromDescriptorFile
                              && NoBundlesContainPath(reference.AssetReference.Path)
                           select CreateAssetReferenceNotFoundMessage(reference.AssetReference);

            var message = string.Join(Environment.NewLine, notFound.ToArray());
            if (message.Length > 0)
            {
                throw new AssetReferenceException(message);
            }
        }

        bool NoBundlesContainPath(string path)
        {
            return !bundles.Any(m => m.ContainsPath(path));
        }

        Dictionary<Bundle, HashedSet<Bundle>> BuildBundleImmediateReferenceDictionary()
        {
            return (
                from bundle in bundles
                select new
                {
                    bundle,
                    references = new HashedSet<Bundle>(GetNonSameBundleAssetReferences(bundle)
                        .Select(r => r.Path)
                        .Concat(bundle.References)
                        .SelectMany(FindBundlesContainingPath).ToList()
                    )
                }
            ).ToDictionary(x => x.bundle, x => x.references);
        }

        IEnumerable<AssetReference> GetNonSameBundleAssetReferences(Bundle bundle)
        {
            var collector = new BundleReferenceCollector(AssetReferenceType.DifferentBundle, AssetReferenceType.Url);
            bundle.Accept(collector);
            return collector.CollectedReferences.Select(r => r.AssetReference);
        }

        string CreateAssetReferenceNotFoundMessage(AssetReference reference)
        {
            if (reference.SourceLineNumber > 0)
            {
                return string.Format(
                    "Reference error in \"{0}\", line {1}. Cannot find \"{2}\".",
                    reference.SourceAsset.Path, reference.SourceLineNumber, reference.Path
                );
            }
            else
            {
                return string.Format(
                    "Reference error in \"{0}\". Cannot find \"{1}\".",
                    reference.SourceAsset.Path, reference.Path
                );
            }
        }

        internal IEnumerable<Bundle> IncludeReferencesAndSortBundles(IEnumerable<Bundle> bundlesToSort)
        {
            if (bundleImmediateReferences == null)
            {
                throw new InvalidOperationException("BuildReferences must be called once before IncludeReferencesAndSortBundles can be called.");
            }

            var bundlesArray = bundlesToSort.ToArray();
            var references = GetBundleReferencesWithImplicitOrderingIncluded(bundlesArray);
            var all = GetAllRequiredBundles(bundlesArray);
            var graph = BuildBundleGraph(references, all);
            return graph.TopologicalSort();
        }

        IEnumerable<Bundle> GetAllRequiredBundles(IEnumerable<Bundle> bundlesArray)
        {
            var all = new HashedSet<Bundle>();
            foreach (var bundle in bundlesArray)
            {
                AddReferencedBundlesToSet(bundle, all);
            }
            return all;
        }

        Graph<Bundle> BuildBundleGraph(IDictionary<Bundle, HashedSet<Bundle>> references, IEnumerable<Bundle> all)
        {
            return new Graph<Bundle>(
                all,
                bundle =>
                {
                    HashedSet<Bundle> set;
                    if (references.TryGetValue(bundle, out set)) return set;
                    return Enumerable.Empty<Bundle>();
                }
            );
        }

        Dictionary<Bundle, HashedSet<Bundle>> GetBundleReferencesWithImplicitOrderingIncluded(IEnumerable<Bundle> initialBundles)
        {
            var roots = initialBundles.Where(m =>
            {
                HashedSet<Bundle> set;
                if (bundleImmediateReferences.TryGetValue(m, out set)) return set.Count == 0;
                return true;
            }).ToList();

            // Clone the original references dictionary, so we can add the extra
            // implicit references based on array order.
            var references = new Dictionary<Bundle, HashedSet<Bundle>>();
            foreach (var reference in bundleImmediateReferences)
            {
                references[reference.Key] = new HashedSet<Bundle>(reference.Value);
            }
            for (var i = 1; i < roots.Count; i++)
            {
                var bundle = roots[i];
                var previous = roots[i - 1];
                HashedSet<Bundle> set;
                if (!references.TryGetValue(bundle, out set))
                {
                    references[bundle] = set = new HashedSet<Bundle>();
                }
                set.Add(previous);
            }
            return references;
        }

        void AddReferencedBundlesToSet(Bundle referencer, ISet<Bundle> all)
        {
            if (all.Contains(referencer)) return;
            all.Add(referencer);

            HashedSet<Bundle> referencedBundles;
            if (!bundleImmediateReferences.TryGetValue(referencer, out referencedBundles)) return;
            foreach (var referencedBundle in referencedBundles)
            {
                AddReferencedBundlesToSet(referencedBundle, all);
            }
        }

        void ThrowIfCyclesInBundleGraph(Graph<Bundle> graph)
        {
            var cycles = graph.FindCycles().ToArray();
            if (cycles.Length > 0)
            {
                ThrowCircularBundleReferenceException(cycles);
            }
        }

        void ThrowCircularBundleReferenceException(IEnumerable<ISet<Bundle>> cycles)
        {
            var details = string.Join(
                Environment.NewLine,
                cycles.Select(cycle => "[" + string.Join(", ", cycle.Select(m => m.Path).ToArray()) + "]").ToArray()
            );
            throw new InvalidOperationException(
                "Cycles detected in bundle dependency graph:" + Environment.NewLine +
                details
            );
        }

        public override bool Equals(object obj)
        {
            var other = obj as BundleCollection;
            if (other == null) return false;

            var thisSorted = this.OrderBy(b => b.GetType().FullName).ThenBy(b => b.Path);
            var otherSorted = other.OrderBy(b => b.GetType().FullName).ThenBy(b => b.Path);
            return thisSorted.SequenceEqual(otherSorted);
        }

        public IEnumerator<Bundle> GetEnumerator()
        {
            return bundles.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        public void Dispose()
        {
            foreach (IDisposable bundle in bundles)
            {
                bundle.Dispose();
            }
        }

        public void Clear()
        {
            bundles.Clear();
        }
    }
}