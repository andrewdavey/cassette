using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;

namespace Cassette
{
    /// <summary>
    /// Watches the source directory for file system changes. Rebuilds the <see cref="BundleCollection"/>.
    /// </summary>
    class FileSystemWatchingBundleRebuilder : IStartUpTask, IDisposable
    {
        readonly CassetteSettings settings;
        readonly BundleCollection bundles;
        readonly BundleCollectionInitializer initializer;
        readonly IEnumerable<IFileSearch> fileSearches;
        IDisposable fileSystemWatcher;
        Timer rebuildDelayTimer;

        public FileSystemWatchingBundleRebuilder(CassetteSettings settings, BundleCollection bundles, BundleCollectionInitializer initializer, IEnumerable<IFileSearch> fileSearches)
        {
            this.settings = settings;
            this.bundles = bundles;
            this.initializer = initializer;
            this.fileSearches = fileSearches;
        }

        /// <summary>
        /// Starts watching the file system for changes.
        /// </summary>
        public void Start()
        {
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
            using (bundles.GetReadLock())
            {
                return fileSearches.Any(fileSearch => fileSearch.IsMatch(path));
            }
        }

        bool IsKnownPath(string path)
        {
            using (bundles.GetReadLock())
            {
                return AnyBundleContainsPath(path) ||
                       RawFileReferenceFinder.RawFileReferenceExists(path, bundles);
            }
        }

        bool AnyBundleContainsPath(string path)
        {
            var predicate = new BundleContainsPathPredicate(path) { AllowPartialAssetPaths = true };
            bundles.Accept(predicate);
            return predicate.Result;
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
            fileSystemWatcher.Dispose();
            rebuildDelayTimer.Dispose();
        }
    }
}