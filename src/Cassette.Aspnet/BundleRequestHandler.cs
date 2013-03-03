using System;
using System.IO;
using System.IO.Compression;
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
            CacheLongTime(actualETag); // Some browsers seem to require a reminder to keep caching?!
            response.StatusCode = 304; // Not Modified
            response.SuppressContent = true;
        }

        void SendBundle(Bundle bundle, string actualETag)
        {
            response.ContentType = bundle.ContentType;
            if(request.RawUrl.Contains(bundle.Hash.ToHexString()))
            {
                CacheLongTime(actualETag);
            }
            else
            {
                NoCache();
            }

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
            response.Cache.SetMaxAge(new TimeSpan(365, 0, 0, 0));
            response.Cache.SetETag(actualETag);
        }

        void NoCache()
        {
            response.AddHeader("Pragma", "no-cache");
            response.CacheControl = "no-cache";
            response.Expires = -1;
        }

        Stream EncodeStreamAndAppendResponseHeaders(Stream stream, string acceptEncoding)
        {
            if (acceptEncoding == null) return stream;

            var preferredEncoding = ParsePreferredEncoding(acceptEncoding);
            if (preferredEncoding == null) return stream;

            response.AppendHeader("Content-Encoding", preferredEncoding);
            response.AppendHeader("Vary", "Accept-Encoding");
            if (preferredEncoding == "deflate")
            {
                return new DeflateStream(stream, CompressionMode.Compress, true);
            }
            if (preferredEncoding == "gzip")
            {
                return new GZipStream(stream, CompressionMode.Compress, true);
            }

            // This line should never be reached because we've filtered out unsupported encoding types.
            // But the compiler doesn't know this. :)
            throw new Exception("Unknown content encoding type \"" + preferredEncoding + "\".");
        }

        static readonly string[] AllowedEncodings = new[] { "gzip", "deflate" };

        static string ParsePreferredEncoding(string acceptEncoding)
        {
            return acceptEncoding
                .Split(new[] { ',' }, StringSplitOptions.RemoveEmptyEntries)
                .Select(type => type.Split(';'))
                .Select(parts => new
                {
                    encoding = parts[0].Trim(),
                    qvalue = ParseQValueFromSecondArrayElement(parts)
                })
                .Where(x => AllowedEncodings.Contains(x.encoding))
                .OrderByDescending(x => x.qvalue)
                .Select(x => x.encoding)
                .FirstOrDefault();
        }

        static float ParseQValueFromSecondArrayElement(string[] parts)
        {
            const float defaultQValue = 1f;
            if (parts.Length < 2) return defaultQValue;

            float qvalue;
            return float.TryParse(parts[1].Trim(), out qvalue) ? qvalue : defaultQValue;
        }
    }
}
