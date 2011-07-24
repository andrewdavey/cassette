using System;
using System.Web;
using System.Web.Caching;

namespace Cassette.Web
{
    public interface ICassetteApplication : IDisposable
    {
        CacheDependency CreateCacheDependency();
        IPageAssetManager CreatePageAssetManager(HttpContextBase httpContext);
        IHttpHandler CreateHttpHandler();
    }
}