using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Cassette.HtmlAppCache
{
    public class CacheManifestHandler : IHttpHandler
    {
        readonly IEnumerable<ICacheManifestProvider> cacheManifestProviders;

        public CacheManifestHandler(IEnumerable<ICacheManifestProvider> cacheManifestProviders)
        {
            if (cacheManifestProviders == null) throw new ArgumentNullException("cacheManifestProviders");
            this.cacheManifestProviders = cacheManifestProviders;
        }

        public void ProcessRequest(HttpContext context)
        {
            var contextWrapper = new HttpContextWrapper(context);
            ProcessRequest(contextWrapper);
        }

        public void ProcessRequest(HttpContextBase context)
        {
            var manifest = GetCacheManifest();
            context.Response.ContentType = "text/cache-manifest";
            context.Response.Write(manifest.ToString());
        }

        CacheManifest GetCacheManifest()
        {
            return cacheManifestProviders
                .Select(p => p.GetCacheManifest())
                .Aggregate(CacheManifest.Merge);
        }

        public bool IsReusable
        {
            get { return true; }
        }
    }
}