using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
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
        readonly IEnumerable<IBundleDefinition> bundleDefinitions;
        FileSystemWatcher watcher;
        Timer rebuildDelayTimer;

        public FileSystemWatchingBundleRebuilder(CassetteSettings settings, BundleCollection bundles, IEnumerable<IBundleDefinition> bundleDefinitions)
        {
            this.settings = settings;
            this.bundles = bundles;
            this.bundleDefinitions = bundleDefinitions;
        }

        /// <summary>
        /// Starts watching the file system for changes.
        /// </summary>
        public void Run()
        {
            var pathToWatch = settings.SourceDirectory.FullPath.Substring(2);
            if (!File.Exists(pathToWatch))
            {
                Trace.Source.TraceEvent(TraceEventType.Warning, 0, "Cannot watch file system for asset file changes because the path does not exist: {0}", settings.SourceDirectory.FullPath);
                return;
            }

            rebuildDelayTimer = new Timer(RebuildDelayTimerCallback);
            watcher = new FileSystemWatcher(pathToWatch)
            {
                IncludeSubdirectories = true,
                NotifyFilter = NotifyFilters.LastWrite | NotifyFilters.FileName | NotifyFilters.DirectoryName,
            };
            watcher.Changed += HandleFileSystemChange;
            watcher.Created += HandleFileSystemChange;
            watcher.Deleted += HandleFileSystemChange;
            watcher.Renamed += HandleFileSystemChange;
            watcher.EnableRaisingEvents = true;
        }

        void HandleFileSystemChange(object sender, FileSystemEventArgs e)
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
                bundles.AddRange(bundleDefinitions);
            }
        }

        /// <summary>
        /// Stops watching the file system for changes.
        /// </summary>
        public void Dispose()
        {
            watcher.EnableRaisingEvents = false;
            watcher.Dispose();
            rebuildDelayTimer.Dispose();
        }
    }
}