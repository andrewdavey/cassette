using System.IO.IsolatedStorage;
using Cassette.Utilities;

namespace Cassette.Aspnet
{
    /// <summary>
    /// Provides the isolated storage used by Cassette.
    /// Storage is only created on demand.
    /// </summary>
    static class IsolatedStorageContainer
    {
        // ReSharper disable ConvertClosureToMethodGroup because would cause this problem: http://stackoverflow.com/q/9113791/7011
        static readonly DisposableLazy<IsolatedStorageFile> LazyStorage = new DisposableLazy<IsolatedStorageFile>(() => CreateIsolatedStorage());
        // ReSharper restore ConvertClosureToMethodGroup

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