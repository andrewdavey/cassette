using System.IO.IsolatedStorage;

namespace Cassette.Web
{
    /// <summary>
    /// Provides the isolated storage used by Cassette.
    /// Storage is only created on demand. 
    /// </summary>
    /// <remarks>
    /// This class cannot be static to make sure this class can be marked as beforefieldinit
    /// http://csharpindepth.com/Articles/General/Beforefieldinit.aspx
    /// </remarks>
    class IsolatedStorageContainer
    {
        public static IsolatedStorageFile IsolatedStorageFile
        {
            get { return LazyContainer.IsolatedStorage; }
        }

        public static void Dispose()
        {
            IsolatedStorageFile.Dispose();
        }

        #region Nested type: LazyContainer

        /// <summary>
        /// A LazyContainer workout due to permission issue using Lazy(T) when initializing IsolatedStorageFile
        /// we are using the Fifth version of the Singleton fully lazy pattern as described in 
        /// http://csharpindepth.com/Articles/General/Singleton.aspx by Jon Skeet
        /// </summary>
        class LazyContainer
        {
            internal static readonly IsolatedStorageFile IsolatedStorage = CreateIsolatedStorage();

            static LazyContainer()
            {
                // Explicit static constructor to tell C# compiler not to mark type as beforefieldinit
            }

            static IsolatedStorageFile CreateIsolatedStorage()
            {
                return IsolatedStorageFile.GetMachineStoreForAssembly();
            }
        }

        #endregion
    }
}