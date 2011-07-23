using System;
using System.Web;
using System.Web.Caching;

namespace Cassette.Web
{
    class SingleManagerContainer
    {
        // Using a static Lazy<T> means we get a singleton Manager which is created in a thread-safe manner.
        static Lazy<ICassetteApplication> managerContainer = new Lazy<ICassetteApplication>(CreateManager);

        public static ICassetteApplication Manager
        {
            get
            {
                return managerContainer.Value;
            }
        }

        static ICassetteApplication CreateManager()
        {
            ICassetteApplication manager;
            try
            {
                manager = new CassetteApplication();
            }
            catch (Exception ex)
            {
                // We don't want to throw the exception during lazy initialization because
                // there is no chance to recreate the object, even if, say, a broken file is fixed.
                // Instead, we cache the exception in this special implementation of IManager
                // and throw it when trying to call members. The CreateCacheDependency()
                // method creates a special dependency that will be triggered just before 
                // the exception is thrown. This provides us with a chance to re-create the
                // lazy object with fixed (we hope!) files.
                manager = new ExceptionCachedManager(ex);
            }
            CacheManagerWithDependency(manager);
            return manager;
        }

        static void CacheManagerWithDependency(ICassetteApplication manager)
        {
            var dependency = manager.CreateCacheDependency();
            HttpRuntime.Cache.Insert(
                "Cassette.Manager",
                manager,
                dependency,
                Cache.NoAbsoluteExpiration,
                Cache.NoSlidingExpiration,
                CacheItemPriority.Normal,
                (key, value, reason) =>
                {
                    IDisposable disposable = value as IDisposable;
                    if (disposable != null) disposable.Dispose();
                    // Manager is removed from cache when the file system is changed.
                    // So we clear the old instance by reassigning the lazy object.
                    // It'll be recreated next time someone requests it.
                    managerContainer = new Lazy<ICassetteApplication>(CreateManager);
                }
            );
        }
    }
}
