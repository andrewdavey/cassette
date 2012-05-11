using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using Cassette.IO;

namespace Cassette
{
    /// <summary>
    /// Watches the source directory for file system changes. Rebuilds the <see cref="BundleCollection"/>.
    /// </summary>
    class FileSystemWatchingBundleRebuilder : IStartUpTask, IDisposable
    {
        readonly CassetteSettings settings;
        readonly BundleCollection bundles;
        readonly IBundleCollectionInitializer initializer;
        readonly IEnumerable<IFileSearch> fileSearches;
        IDisposable fileSystemWatcher;
        Timer rebuildDelayTimer;

        public FileSystemWatchingBundleRebuilder(CassetteSettings settings, BundleCollection bundles, IBundleCollectionInitializer initializer, IEnumerable<IFileSearch> fileSearches)
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
            if (IsCacheFile(path)) return false;
            try
            {
                using (bundles.GetReadLock())
                {
                    return fileSearches.Any(fileSearch => fileSearch.IsMatch(path));
                }
            }
            catch (Exception exception)
            {
                // Swallow the exception, otherwise it will bubble up unhandled and kill the process!
                Trace.Source.TraceData(TraceEventType.Error, 0, exception);
                return false;
            }
        }

        bool IsKnownPath(string path)
        {
            if (IsCacheFile(path)) return false;
            try
            {
                using (bundles.GetReadLock())
                {
                    return AnyBundleContainsPath(path) ||
                           RawFileReferenceFinder.RawFileReferenceExists(path, bundles);
                }
            }
            catch (Exception exception)
            {
                // Swallow the exception, otherwise it will bubble up unhandled and kill the process!
                Trace.Source.TraceData(TraceEventType.Error, 0, exception);
                return false;
            }
        }

        bool AnyBundleContainsPath(string path)
        {
            var predicate = new BundleContainsPathPredicate(path) { AllowPartialAssetPaths = true };
            bundles.Accept(predicate);
            return predicate.Result;
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
            fileSystemWatcher.Dispose();
            rebuildDelayTimer.Dispose();
        }
    }
}