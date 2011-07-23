using System.Web;
using System.Web.Caching;

namespace Cassette.Web
{
    public interface IManager
    {
        CacheDependency CreateCacheDependency();
        IPageHelper CreatePageHelper(HttpContextBase httpContext);
        IHttpHandler CreateHttpHandler();
    }
}