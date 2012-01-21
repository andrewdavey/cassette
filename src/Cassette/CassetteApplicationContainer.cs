using System;
using System.IO;

namespace Cassette
{
    /// <summary>
    /// Provides access to the current Cassette application object.
    /// </summary>
    public static class CassetteApplicationContainer
    {
        static ICassetteApplicationContainer<ICassetteApplication> _containerSingleton;

        /// <summary>
        /// Gets the current <see cref="ICassetteApplication"/> used by Cassette.
        /// </summary>
        public static ICassetteApplication Application
        {
            get { return _containerSingleton.Application; }
        }

        /// <summary>
        /// Sets the container used to access the current Cassette application object.
        /// Unit tests can use this method to assign a stub container for testing purposes.
        /// </summary>
        public static void SetContainerSingleton(ICassetteApplicationContainer<ICassetteApplication> containerSingleton)
        {
            _containerSingleton = containerSingleton;
        }
    }

    class CassetteApplicationContainer<T> : ICassetteApplicationContainer<T>
        where T : ICassetteApplication
    {
        public static T Application
        {
            get { return (T)CassetteApplicationContainer.Application; }
        }

        readonly Func<T> createApplication;
        FileSystemWatcher watcher;
        Lazy<T> application;
        bool creationFailed;

        public CassetteApplicationContainer(Func<T> createApplication)
        {
            this.createApplication = createApplication;
            application = new Lazy<T>(CreateApplication);
        }

        public CassetteApplicationContainer(Func<T> createApplication, string rootDirectoryToWatch)
            : this(createApplication)
        {

            // In production mode we don't expect the asset files to change
            // while the application is running. Changes to assets will involve a 
            // re-deployment and restart of the app pool. So new assets are loaded then.

            // In development mode, asset files will likely change while application is
            // running. So watch the file system and recycle the application object 
            // when files are created/changed/deleted/etc.
            StartWatchingFileSystem(rootDirectoryToWatch);
        }

        T ICassetteApplicationContainer<T>.Application
        {
            get
            {
                return application.Value;
            }
        }

        void StartWatchingFileSystem(string rootDirectoryToWatch)
        {
            watcher = new FileSystemWatcher(rootDirectoryToWatch)
            {
                IncludeSubdirectories = true,
                EnableRaisingEvents = true
            };
            watcher.Created += HandleFileSystemChange;
            watcher.Changed += HandleFileSystemChange;
            watcher.Renamed += HandleFileSystemChange;
            watcher.Deleted += HandleFileSystemChange;
        }

        void HandleFileSystemChange(object sender, FileSystemEventArgs e)
        {
            RecycleApplication();
        }

        public void RecycleApplication()
        {
            if (IsPendingCreation) return; // Already recycled, awaiting first creation.

            lock (this)
            {
                if (IsPendingCreation) return;

                if (creationFailed)
                {
                    creationFailed = false;
                }
                else
                {
                    application.Value.Dispose();
                }
                // Re-create the lazy object. So the application isn't created until it's asked for.
                application = new Lazy<T>(CreateApplication);
            }
        }

        internal void ForceApplicationCreation()
        {
// ReSharper disable UnusedVariable
            var forceCreation = application.Value;
// ReSharper restore UnusedVariable
        }

        bool IsPendingCreation
        {
            get { return creationFailed == false && application.IsValueCreated == false; }
        }

        T CreateApplication()
        {
            try
            {
                var app = createApplication();
                creationFailed = false;
                return app;
            }
            catch
            {
                creationFailed = true;
                throw;
            }
        }

        public void Dispose()
        {
            if (watcher != null)
            {
                watcher.Dispose();
            }
            if (application.IsValueCreated)
            {
                application.Value.Dispose();
            }
        }
    }
}