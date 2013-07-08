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
            LazyStorage = perDomain ?
                new DisposableLazy<IsolatedStorageFile>(() => IsolatedStorageFile.GetMachineStoreForDomain()) : 
                new DisposableLazy<IsolatedStorageFile>(() => IsolatedStorageFile.GetMachineStoreForAssembly());
            // ReSharper restore ConvertClosureToMethodGroup
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