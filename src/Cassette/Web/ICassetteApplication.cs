using System.Web;
using System.Web.Caching;

namespace Cassette.Web
{
    public interface ICassetteApplication
    {
        CacheDependency CreateCacheDependency();
        IPageAssetManager CreatePageHelper(HttpContextBase httpContext);
        IHttpHandler CreateHttpHandler();
    }
}