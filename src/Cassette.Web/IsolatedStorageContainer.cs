using System.IO.IsolatedStorage;

namespace Cassette.Web
{
    /// <summary>
    /// Provides the isolated storage used by Cassette.
    /// Storage is only created on demand.
    /// </summary>
    static class IsolatedStorageContainer
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

        public static void Dispose()
        {
            LazyStorage.Dispose();
        }
    }
}