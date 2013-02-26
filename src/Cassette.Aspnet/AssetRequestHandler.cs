using System.Web;
using System.Web.Routing;
using Cassette.Utilities;
using Trace = Cassette.Diagnostics.Trace;
using System;

namespace Cassette.Aspnet
{
    class AssetRequestHandler : ICassetteRequestHandler
    {
        public AssetRequestHandler(RequestContext requestContext, BundleCollection bundles)
        {
            this.requestContext = requestContext;
            this.bundles = bundles;
        }

        readonly RequestContext requestContext;
        readonly BundleCollection bundles;

        public void ProcessRequest(string path)
        {
            Trace.Source.TraceInformation("Handling asset request for path \"{0}\".", path);
            requestContext.HttpContext.DisableHtmlRewriting();
            var response = requestContext.HttpContext.Response;
            using (bundles.GetReadLock())
            {
                Bundle bundle;
                IAsset asset;
                if (!bundles.TryGetAssetByPath(path, out asset, out bundle))
                {
                    Trace.Source.TraceInformation("Bundle asset not found with path \"{0}\".", path);
                    NotFound(response);
                    return;
                }

                var request = requestContext.HttpContext.Request;
                SendAsset(request, response, bundle, asset);
            }
        }

        void SendAsset(HttpRequestBase request, HttpResponseBase response, Bundle bundle, IAsset asset)
        {
            response.ContentType = bundle.ContentType;

            var actualETag = "\"" + asset.Hash.ToHexString() + "\"";
            if(request.RawUrl.Contains(asset.Hash.ToHexString())) {
                CacheLongTime(response, actualETag);
            }
            else {
                NoCache(response);
            }

            var givenETag = request.Headers["If-None-Match"];
            if (givenETag == actualETag)
            {
                SendNotModified(response);
            }
            else
            {
                using (var stream = asset.OpenStream())
                {
                    stream.CopyTo(response.OutputStream);
                }
            }
        }

        void CacheLongTime(HttpResponseBase response, string actualETag)
        {
            response.Cache.SetCacheability(HttpCacheability.Public);
            response.Cache.SetExpires(DateTime.UtcNow.AddYears(1));
            response.Cache.SetMaxAge(new TimeSpan(365, 0, 0, 0));
            response.Cache.SetETag(actualETag);
        }

        void NoCache(HttpResponseBase response) {
            response.AddHeader("Pragma", "no-cache");
            response.CacheControl = "no-cache";
            response.Expires = -1;
        }

        void SendNotModified(HttpResponseBase response)
        {
            response.StatusCode = 304; // Not Modified
            response.SuppressContent = true;
        }

        void NotFound(HttpResponseBase response)
        {
            response.StatusCode = 404;
            response.SuppressContent = true;
        }
    }
}