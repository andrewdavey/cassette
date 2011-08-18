using System;
using System.IO;
using System.IO.Compression;
using System.Web;
using System.Web.Routing;
using Cassette.Utilities;

namespace Cassette.Web
{
    public class ModuleRequestHandler<T> : IHttpHandler
        where T : Module
    {
        public ModuleRequestHandler(IModuleContainer<T> moduleContainer, RequestContext requestContext)
        {
            this.moduleContainer = moduleContainer;
            
            routeData = requestContext.RouteData;
            response = requestContext.HttpContext.Response;
            request = requestContext.HttpContext.Request;
        }

        readonly IModuleContainer<T> moduleContainer;
        readonly RouteData routeData;
        readonly HttpResponseBase response;
        readonly HttpRequestBase request;

        public void ProcessRequest()
        {
            var module = FindModule();
            if (module == null)
            {
                response.StatusCode = 404;
            }
            else
            {
                var actualETag = module.Assets[0].Hash.ToHexString();
                var givenETag = request.Headers["If-None-Match"];
                if (givenETag == actualETag)
                {
                    SendNotModified(actualETag);
                }
                else
                {
                    SendModule(module, actualETag);
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

        T FindModule()
        {
            var path = routeData.GetRequiredString("path");
            path = RemoveTrailingHashFromPath(path);
            return moduleContainer.FindModuleContainingPath(path);
        }

        /// <summary>
        /// A Module URL has the hash appended after an underscore character. This method removes the underscore and hash from the path.
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

        void SendModule(T module, string actualETag)
        {
            response.ContentType = module.ContentType;
            CacheLongTime(actualETag);

            var encoding = request.Headers["Accept-Encoding"];
            response.Filter = EncodeStreamAndAppendResponseHeaders(response.Filter, encoding);

            using (var assetStream = module.Assets[0].OpenStream())
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