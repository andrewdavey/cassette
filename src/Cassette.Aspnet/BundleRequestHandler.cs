using System.Linq;
using System.Net;
using System.Web;
using System.Web.Routing;
using Cassette.Utilities;
using Trace = Cassette.Diagnostics.Trace;

namespace Cassette.Aspnet
{
    internal class BundleRequestHandler<T> : ICassetteRequestHandler
        where T : Bundle
    {
        readonly BundleCollection bundles;
        readonly HttpResponseBase response;
        readonly HttpRequestBase request;
        readonly HttpContextBase httpContext;

        public BundleRequestHandler(BundleCollection bundles, RequestContext requestContext)
        {
            this.bundles = bundles;

            response = requestContext.HttpContext.Response;
            request = requestContext.HttpContext.Request;
            httpContext = requestContext.HttpContext;
        }

        public void ProcessRequest(string path)
        {
            httpContext.DisableHtmlRewriting();
            using (bundles.GetReadLock())
            {
                var bundle = FindBundle(path);
                if (bundle == null)
                {
                    Trace.Source.TraceInformation("Bundle not found \"{0}\".", path);
                    response.StatusCode = (int) HttpStatusCode.NotFound;
                    throw new HttpException((int) HttpStatusCode.NotFound, "Bundle not found");
                }

                var actualETag = "\"" + bundle.Hash.ToHexString() + "\"";
                var givenETag = request.Headers["If-None-Match"];
                if (givenETag == actualETag)
                {
                    SendNotModified(actualETag);
                }
                else
                {
                    SendBundle(bundle, actualETag);
                }
            }
        }

        Bundle FindBundle(string path)
        {
            Trace.Source.TraceInformation("Handling bundle request for \"{0}\".", path);
            return bundles.FindBundlesContainingPath(path).OfType<T>().FirstOrDefault();
        }

        void SendNotModified(string actualETag)
        {
            HttpResponseUtil.CacheLongTime(response, actualETag); // Some browsers seem to require a reminder to keep caching?!
            response.StatusCode = 304; // Not Modified
            response.SuppressContent = true;
        }

        void SendBundle(Bundle bundle, string actualETag)
        {
            response.ContentType = bundle.ContentType;
            if(request.RawUrl.Contains(bundle.Hash.ToHexString()))
            {
                HttpResponseUtil.CacheLongTime(response, actualETag);
            }
            else
            {
                HttpResponseUtil.NoCache(response);
            }

            HttpResponseUtil.EncodeStreamAndAppendResponseHeaders(request, response);

            using (var assetStream = bundle.OpenStream())
            {
                assetStream.CopyTo(response.OutputStream);
            }
        }
    }
}
