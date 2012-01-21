using System.ComponentModel;
using System.IO.IsolatedStorage;

[assembly: WebActivator.ApplicationShutdownMethod(typeof(Cassette.Web.IsolatedStorageContainer), "ApplicationShutdown")]

namespace Cassette.Web
{
    /// <summary>
    /// Provides the isolated storage used by Cassette.
    /// Storage is only created on demand.
    /// On web application shutdown the storage object is disposed.
    /// </summary>
    /// <remarks>
    /// Class is public only because it's called by WebActivator using reflection.
    /// Reflection can only call public methods when the application is not running with full trust.
    /// </remarks>
    [EditorBrowsable(EditorBrowsableState.Never)]
    public static class IsolatedStorageContainer
    {
        static readonly DisposableLazy<IsolatedStorageFile> LazyStorage = new DisposableLazy<IsolatedStorageFile>(CreateIsolatedStorage);

        static IsolatedStorageFile CreateIsolatedStorage()
        {
            return IsolatedStorageFile.GetMachineStoreForAssembly();
        }

        public static IsolatedStorageFile IsolatedStorageFile
        {
            get { return LazyStorage.Value; }
        }

        // ReSharper disable UnusedMember.Global
        public static void ApplicationShutdown()
        {
            LazyStorage.Dispose();
        }
        // ReSharper restore UnusedMember.Global
    }
}
