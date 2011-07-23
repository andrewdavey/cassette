using System;
using System.Web.Caching;
using Cassette.CoffeeScript;
using Cassette.Configuration;
using System.Web;

namespace Cassette.Web
{
    // We don't want to throw the exception during lazy initialization because
    // there is no chance to recreate the object, even if, say, a broken file is fixed.
    // Instead, we cache the exception in this special implementation of IManager
    // and throw it when trying to call members. The CreateCacheDependency()
    // method creates a special dependency that will be triggered just before 
    // the exception is thrown. This provides us with a chance to re-create the
    // lazy object with fixed (we hope!) files.
    public class ExceptionCachedManager : ICassetteApplication
    {
        public ExceptionCachedManager(Exception exception)
        {
            this.exception = exception;
            cacheClearer = new CacheClearer();
        }

        readonly Exception exception;
        readonly CacheClearer cacheClearer;

        public IPageAssetManager CreatePageHelper(HttpContextBase httpContext)
        {
            cacheClearer.Clear();
            throw exception;
        }

        public IHttpHandler CreateHttpHandler()
        {
            cacheClearer.Clear();
            throw exception;
        }

        public CacheDependency CreateCacheDependency()
        {
            return cacheClearer;
        }

        class CacheClearer : CacheDependency
        {
            public void Clear()
            {
                NotifyDependencyChanged(this, EventArgs.Empty);
            }
        }
    }
}
