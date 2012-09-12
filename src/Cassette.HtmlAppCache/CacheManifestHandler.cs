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
            if (IsNotModified(context, manifest))
            {
                SendNotModified(context);
            }
            else
            {
                SendCacheManifest(manifest, context);
            }
        }

        bool IsNotModified(HttpContextBase context, CacheManifest manifest)
        {
            DateTime ifModifiedSince;
            var header = context.Request.Headers["If-Modified-Since"];
            var parsedDate = DateTime.TryParse(header, out ifModifiedSince);
            if (!parsedDate) return false;

            var currentModified = RemoveFractionalSeconds(manifest.LastModified);
            return currentModified <= ifModifiedSince.ToUniversalTime();
        }

        DateTime RemoveFractionalSeconds(DateTime time)
        {
            return new DateTime(time.Year, time.Month, time.Day, time.Hour, time.Minute, time.Second, 0, DateTimeKind.Utc);
        }

        void SendCacheManifest(CacheManifest manifest, HttpContextBase context)
        {
            context.Response.ContentType = "text/cache-manifest";
            context.Response.Cache.SetLastModified(manifest.LastModified);
            context.Response.Write(manifest.ToString());
        }

        void SendNotModified(HttpContextBase context)
        {
            context.Response.StatusCode = 304; // Not Modified
            context.Response.SuppressContent = true;
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