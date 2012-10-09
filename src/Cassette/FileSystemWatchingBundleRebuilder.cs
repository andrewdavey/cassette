using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading;
using Cassette.IO;
using Cassette.Utilities;
using Trace = Cassette.Diagnostics.Trace;

namespace Cassette
{
    /// <summary>
    /// Watches the source directory for file system changes. Rebuilds the <see cref="BundleCollection"/>.
    /// </summary>
    public class FileSystemWatchingBundleRebuilder : IStartUpTask, IDisposable
    {
        readonly CassetteSettings settings;
        readonly BundleCollection bundles;
        readonly IBundleCollectionInitializer initializer;
        readonly IEnumerable<IFileSearch> fileSearches;
        readonly HashedCompareSet<string> bundleDescriptorFilenames;
        IDisposable fileSystemWatcher;
        Timer rebuildDelayTimer;
        IEnumerable<Bundle> readOnlyBundles;
        /// <summary>
        /// This lock protects the readOnlyBundles field.
        /// </summary>
        readonly ReaderWriterLockSlim readOnlyBundlesLock = new ReaderWriterLockSlim();

        public FileSystemWatchingBundleRebuilder(CassetteSettings settings, BundleCollection bundles, IBundleCollectionInitializer initializer, IEnumerable<IFileSearch> fileSearches)
        {
            this.settings = settings;
            this.bundles = bundles;
            this.initializer = initializer;
            this.fileSearches = fileSearches;

            bundleDescriptorFilenames = GetBundleDescriptorFilenames();

            // Initially use the bundles collection, but this will get updated in the Changed event handler.
            readOnlyBundles = new ReadOnlyCollection<Bundle>(bundles.ToList());
            bundles.Changed += HandleBundlesChanged;
        }

        void HandleBundlesChanged(object sender, BundleCollectionChangedEventArgs e)
        {
            readOnlyBundlesLock.EnterWriteLock();
            try
            {
                readOnlyBundles = e.Bundles;
            }
            finally
            {
                readOnlyBundlesLock.ExitWriteLock();
            }
        }

        static HashedCompareSet<string> GetBundleDescriptorFilenames()
        {
            var bundleTypes = new[]
            {
                typeof(Bundle),
                typeof(Scripts.ScriptBundle),
                typeof(Stylesheets.StylesheetBundle),
                typeof(HtmlTemplates.HtmlTemplateBundle)
            };
            return new HashedCompareSet<string>(bundleTypes.Select(type => type.Name + ".txt").ToArray(), StringComparer.OrdinalIgnoreCase);
        }

        /// <summary>
        /// Starts watching the file system for changes.
        /// </summary>
        public void Start()
        {
            if (!settings.IsFileSystemWatchingEnabled) return;

            rebuildDelayTimer = new Timer(RebuildDelayTimerCallback);
            fileSystemWatcher = settings.SourceDirectory.WatchForChanges(
                HandleCreated,
                HandleChanged,
                HandleDeleted,
                HandleRenamed
            );
        }

        void HandleCreated(string path)
        {
            if (IsPotentialAssetFile(path))
            {
                QueueRebuild();
            }
        }

        void HandleChanged(string path)
        {
            if (IsKnownPath(path))
            {
                QueueRebuild();
            }
        }

        void HandleDeleted(string path)
        {
            if (IsKnownPath(path))
            {
                QueueRebuild();
            }
        }

        void HandleRenamed(string oldPath, string newPath)
        {
            if (IsPotentialAssetFile(newPath) || IsKnownPath(oldPath))
            {
                QueueRebuild();
            }
        }

        bool IsPotentialAssetFile(string path)
        {
            if (IsBundleDescriptorFile(path)) return true;
            if (IsCacheFile(path)) return false;
            return fileSearches.Any(fileSearch => fileSearch.IsMatch(path));
        }

        bool IsBundleDescriptorFile(string path)
        {
            var filename = path.Split('/', '\\').Last();
            return bundleDescriptorFilenames.Contains(filename);
        }

        bool IsKnownPath(string path)
        {
            if (IsBundleDescriptorFile(path)) return true;
            if (IsCacheFile(path)) return false;
            return AnyBundleContainsPath(path) ||
                   RawFileReferenceExists(path);
        }

        bool AnyBundleContainsPath(string path)
        {
            readOnlyBundlesLock.EnterReadLock();
            try
            {
                var predicate = new BundleContainsPathPredicate(path) { AllowPartialAssetPaths = true };
                readOnlyBundles.Accept(predicate);
                return predicate.Result;
            }
            finally
            {
                readOnlyBundlesLock.ExitReadLock();
            }
        }

        bool RawFileReferenceExists(string path)
        {
            readOnlyBundlesLock.EnterReadLock();
            try
            {
                return RawFileReferenceFinder.RawFileReferenceExists(path, readOnlyBundles);
            }
            finally
            {
                readOnlyBundlesLock.ExitReadLock();                
            }
        }

        bool IsCacheFile(string path)
        {
            // path is relative to source directory. So to be a path in cache, cache needs to be contained within source.
            // The following is a bit of hack.
            var cache = settings.CacheDirectory as FileSystemDirectory;
            var source = settings.SourceDirectory as FileSystemDirectory;
            if (cache == null || source == null) return false;
            var subDirectory = source.TryGetAsSubDirectory(cache);
            if (subDirectory != null)
            {
                return path.StartsWith(subDirectory.FullPath, StringComparison.OrdinalIgnoreCase);
            }
            else
            {
                return false;
            }
        }

        void QueueRebuild()
        {
            // We tend to get a few file change events in quick succession.
            // So rather than rebuild the bundles repeatedly, 
            // we wait delay by 100ms to allow all the events to happen, then rebuild the bundles.
            rebuildDelayTimer.Change(100, Timeout.Infinite);
        }

        void RebuildDelayTimerCallback(object state)
        {
            RebuildBundles();
        }

        void RebuildBundles()
        {
            Trace.Source.TraceInformation("Rebuilding bundles due to file system changes.");
            initializer.Initialize(bundles);
        }

        /// <summary>
        /// Stops watching the file system for changes.
        /// </summary>
        public void Dispose()
        {
            var fileSystemWatcherCopy = fileSystemWatcher;
            if (fileSystemWatcherCopy != null)
            {
                fileSystemWatcherCopy.Dispose();
            }

            var rebuildDelayTimerCopy = rebuildDelayTimer;
            if (rebuildDelayTimerCopy != null)
            {
                rebuildDelayTimerCopy.Dispose();
            }
        }
    }
}