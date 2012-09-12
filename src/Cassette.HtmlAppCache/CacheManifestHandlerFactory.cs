using System;
using System.Web;

namespace Cassette.HtmlAppCache
{
    public class CacheManifestHandlerFactory : IHttpHandlerFactory
    {
        // Assigned by StartUp
        public static Func<CacheManifestHandler> Factory { get; set; }

        public IHttpHandler GetHandler(HttpContext context, string requestType, string url, string pathTranslated)
        {
            return Factory();
        }

        public void ReleaseHandler(IHttpHandler handler)
        {
        }
    }
}