using System.IO.IsolatedStorage;
using Cassette.Utilities;

namespace Cassette.Aspnet
{
    /// <summary>
    /// Provides the isolated storage used by Cassette.
    /// Storage is only created on demand.
    /// </summary>
    class IsolatedStorageContainer
    {
        readonly DisposableLazy<IsolatedStorageFile> LazyStorage;

        public IsolatedStorageContainer(bool perDomain)
        {
            // ReSharper disable ConvertClosureToMethodGroup because would cause this problem: http://stackoverflow.com/q/9113791/7011
            LazyStorage = new DisposableLazy<IsolatedStorageFile>(() => CreateIsolatedStorage(perDomain));
            // ReSharper restore ConvertClosureToMethodGroup
        }

        static IsolatedStorageFile CreateIsolatedStorage(bool perDomain)
        {
            return perDomain 
                ? IsolatedStorageFile.GetMachineStoreForDomain() 
                : IsolatedStorageFile.GetMachineStoreForAssembly();
        }

        public IsolatedStorageFile IsolatedStorageFile
        {
            get { return LazyStorage.Value; }
        }

        public void Dispose()
        {
            LazyStorage.Dispose();
        }
    }
}