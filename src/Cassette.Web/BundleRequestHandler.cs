using System;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Web;
using System.Web.Routing;
using Cassette.Utilities;

namespace Cassette.Web
{
    class BundleRequestHandler<T> : IHttpHandler
        where T : Bundle
    {
        public BundleRequestHandler(Func<IBundleContainer> getBundleContainer, RequestContext requestContext)
        {
            this.getBundleContainer = getBundleContainer;
            
            routeData = requestContext.RouteData;
            response = requestContext.HttpContext.Response;
            request = requestContext.HttpContext.Request;
            httpContext = requestContext.HttpContext;
        }

        readonly Func<IBundleContainer> getBundleContainer;
        readonly RouteData routeData;
        readonly HttpResponseBase response;
        readonly HttpRequestBase request;
        readonly HttpContextBase httpContext;

        public void ProcessRequest()
        {
            httpContext.DisableHtmlRewriting();
            var bundle = FindBundle();
            if (bundle == null)
            {
                Trace.Source.TraceInformation("Bundle not found \"{0}\".", Path.Combine("~", routeData.GetRequiredString("path")));
                response.StatusCode = 404;
            }
            else
            {
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

        public bool IsReusable
        {
            get { return false; }
        }

        void IHttpHandler.ProcessRequest(HttpContext unused)
        {
            // The HttpContext is unused because the constructor accepts a more test-friendly RequestContext object.
            ProcessRequest();
        }

        Bundle FindBundle()
        {
            var path = "~/" + routeData.GetRequiredString("path");
            Trace.Source.TraceInformation("Handling bundle request for \"{0}\".", path);
            path = RemoveTrailingHashFromPath(path);
            return getBundleContainer().FindBundlesContainingPath(path).OfType<T>().FirstOrDefault();
        }

        /// <summary>
        /// A Bundle URL has the hash appended after an underscore character. This method removes the underscore and hash from the path.
        /// </summary>
        string RemoveTrailingHashFromPath(string path)
        {
            var index = path.LastIndexOf('_');
            if (index >= 0)
            {
                return path.Substring(0, index);
            }
            return path;
        }

        void SendNotModified(string actualETag)
        {
            CacheLongTime(actualETag); // Some browsers seem to require a reminder to keep caching?!
            response.StatusCode = 304; // Not Modified
            response.SuppressContent = true;
        }

        void SendBundle(Bundle bundle, string actualETag)
        {
            response.ContentType = bundle.ContentType;
            CacheLongTime(actualETag);

            var encoding = request.Headers["Accept-Encoding"];
            response.Filter = EncodeStreamAndAppendResponseHeaders(response.Filter, encoding);
            
            using (var assetStream = bundle.OpenStream())
            {
                assetStream.CopyTo(response.OutputStream);
            }
        }

        void CacheLongTime(string actualETag)
        {
            response.Cache.SetCacheability(HttpCacheability.Public);
            response.Cache.SetExpires(DateTime.UtcNow.AddYears(1));
            response.Cache.SetETag(actualETag);
        }

        Stream EncodeStreamAndAppendResponseHeaders(Stream stream, string encoding)
        {
            if (encoding == null)
            {
                return stream;
            }

            if (encoding.IndexOf("deflate", StringComparison.OrdinalIgnoreCase) >= 0)
            {
                response.AppendHeader("Content-Encoding", "deflate");
                response.AppendHeader("Vary", "Accept-Encoding");
                return new DeflateStream(stream, CompressionMode.Compress, true);
            }
            else if (encoding.IndexOf("gzip", StringComparison.OrdinalIgnoreCase) >= 0)
            {
                response.AppendHeader("Content-Encoding", "gzip");
                response.AppendHeader("Vary", "Accept-Encoding");
                return new GZipStream(stream, CompressionMode.Compress, true);
            }
            return stream;
        }
    }
}
