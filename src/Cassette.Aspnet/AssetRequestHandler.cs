using System.Net;
using System.Web;
using System.Web.Routing;
using Cassette.Utilities;
using Trace = Cassette.Diagnostics.Trace;

namespace Cassette.Aspnet
{
    class AssetRequestHandler : ICassetteRequestHandler
    {
        public AssetRequestHandler(RequestContext requestContext, BundleCollection bundles)
        {
            this.requestContext = requestContext;
            this.bundles = bundles;
            this.response = requestContext.HttpContext.Response;
            this.request = requestContext.HttpContext.Request;
        }

        readonly RequestContext requestContext;
        readonly BundleCollection bundles;
        readonly HttpResponseBase response;
        readonly HttpRequestBase request;

        public void ProcessRequest(string path)
        {
            Trace.Source.TraceInformation("Handling asset request for path \"{0}\".", path);
            requestContext.HttpContext.DisableHtmlRewriting();
            using (bundles.GetReadLock())
            {
                Bundle bundle;
                IAsset asset;
                if (!bundles.TryGetAssetByPath(path, out asset, out bundle))
                {
                    Trace.Source.TraceInformation("Bundle asset not found with path \"{0}\".", path);
                    response.StatusCode = (int) HttpStatusCode.NotFound;
                    throw new HttpException((int) HttpStatusCode.NotFound, "Asset not found");
                }

                SendAsset(bundle, asset);
            }
        }

        void SendAsset(Bundle bundle, IAsset asset)
        {
            response.ContentType = bundle.ContentType;

            var actualETag = "\"" + asset.Hash.ToHexString() + "\"";
            if(request.RawUrl.Contains(asset.Hash.ToHexString())) {
                HttpResponseUtil.CacheLongTime(response, actualETag);
            }
            else {
                HttpResponseUtil.NoCache(response);
            }

            var givenETag = request.Headers["If-None-Match"];
            if (givenETag == actualETag)
            {
                HttpResponseUtil.SendNotModified(response);
            }
            else
            {
                HttpResponseUtil.EncodeStreamAndAppendResponseHeaders(request, response);

                using (var stream = asset.OpenStream())
                {
                    stream.CopyTo(response.OutputStream);
                }
            }
        }
    }
}