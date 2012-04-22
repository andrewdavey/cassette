using System.Web;
using System.Web.Routing;
using Cassette.Utilities;

namespace Cassette.Aspnet
{
    class AssetRequestHandler : IHttpHandler
    {
        public AssetRequestHandler(RequestContext requestContext, BundleCollection bundles)
        {
            this.requestContext = requestContext;
            this.bundles = bundles;
        }

        readonly RequestContext requestContext;
        readonly BundleCollection bundles;

        public bool IsReusable
        {
            get { return false; }
        }

        public void ProcessRequest(HttpContext _)
        {
            var path = "~/" + requestContext.RouteData.GetRequiredString("path");
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
            response.Cache.SetCacheability(HttpCacheability.Public);
            response.Cache.SetETag(actualETag);

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