using System;
using System.Collections.Generic;
using System.Threading;
using Cassette.Configuration;

namespace Cassette
{
    /// <summary>
    /// Watches the source directory for file system changes. Rebuilds the <see cref="BundleCollection"/>.
    /// </summary>
    class FileSystemWatchingBundleRebuilder : IStartUpTask, IDisposable
    {
        readonly CassetteSettings settings;
        readonly BundleCollection bundles;
        readonly IEnumerable<IConfiguration<BundleCollection>> bundleConfigurations;
        IDisposable fileSystemWatcher;
        Timer rebuildDelayTimer;

        public FileSystemWatchingBundleRebuilder(CassetteSettings settings, BundleCollection bundles, IEnumerable<IConfiguration<BundleCollection>> bundleConfigurations)
        {
            this.settings = settings;
            this.bundles = bundles;
            this.bundleConfigurations = bundleConfigurations;
        }

        /// <summary>
        /// Starts watching the file system for changes.
        /// </summary>
        public void Start()
        {
            rebuildDelayTimer = new Timer(RebuildDelayTimerCallback);
            fileSystemWatcher = settings.SourceDirectory.WatchForChanges(HandleFileSystemChange);
        }

        void HandleFileSystemChange(string path)
        {
            // TODO: Ignore App_Data (and other special directories?)

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
            using (bundles.GetWriteLock())
            {
                bundles.Clear();
                bundles.AddRange(bundleConfigurations);
            }
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