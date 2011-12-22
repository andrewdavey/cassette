using System;
using System.Collections.Generic;
using System.IO;
using System.Security.Cryptography;
using System.Text.RegularExpressions;
using System.Web;
using System.Web.Routing;
using System.Diagnostics;

namespace Cassette.Web
{
    class RawFileRequestHandler : IHttpHandler
    {
        public RawFileRequestHandler(RequestContext requestContext)
        {
            routeData = requestContext.RouteData;
            response = requestContext.HttpContext.Response;
            request = requestContext.HttpContext.Request;
            server = requestContext.HttpContext.Server;
        }

        readonly RouteData routeData;
        readonly HttpResponseBase response;
        readonly HttpRequestBase request;
        readonly HttpServerUtilityBase server;
        
        static readonly Dictionary<string, string> ContentTypes =
            new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)
            {
                { "png", "image/png" },
                { "jpg", "image/jpeg" },
                { "jpeg", "image/jpeg" },
                { "gif", "image/gif" },
                { "eot", "application/vnd.ms-fontobject" },
                { "otf", "application/octet-stream" },
                { "ttf", "application/octet-stream" },
                { "woff", "application/x-font-woff" },
                { "svg", "image/svg+xml" }
            };

        public bool IsReusable
        {
            get { return false; }
        }

        public void ProcessRequest(HttpContext unused)
        {
            var path = routeData.GetRequiredString("path");
            var match = Regex.Match(path, @"^(?<filename>.*)_[a-z0-9]+_(?<extension>[a-z]+)$", RegexOptions.IgnoreCase);
            if (match.Success == false)
            {
                NotFound(path);
                return;
            }

            var extension = match.Groups["extension"].Value;
            var filename = match.Groups["filename"].Value + "." + extension;
            var fullPath = server.MapPath("~/" + filename);
            if (File.Exists(fullPath))
            {
                SendFile(fullPath, extension);
            }
            else
            {
                Trace.Source.TraceEvent(TraceEventType.Error, 0, "File not found \"{0}\".", fullPath);
                response.StatusCode = 404;
            }
        }

        void NotFound(string path)
        {
            Trace.Source.TraceEvent(TraceEventType.Error, 0, "Invalid file path in URL \"{0}\".", path);
            response.StatusCode = 404;
        }

        void SendFile(string fullPath, string extension)
        {
            var eTag = GetETag(fullPath);
            response.Cache.SetCacheability(HttpCacheability.Public);
            response.Cache.SetExpires(DateTime.Now.AddYears(1));
            response.Cache.SetETag(eTag);

            var requestETag = request.Headers["If-None-Match"];
            if (requestETag == eTag)
            {
                NotModified();
                return;
            }

            SetResponseContentType(fullPath, extension);
            response.WriteFile(fullPath);
        }

        void NotModified()
        {
            response.StatusCode = 304; // Not Modified
            response.SuppressContent = true;
        }

        void SetResponseContentType(string fullPath, string extension)
        {
            var contentType = ContentTypeFromExtension(extension);
            if (contentType != null)
            {
                Trace.Source.TraceEvent(
                    TraceEventType.Error,
                    0,
                    "Sending file \"{0}\" with content type {1}.",
                    fullPath,
                    response.ContentType
                    );
            }
            else
            {
                Trace.Source.TraceEvent(
                    TraceEventType.Warning,
                    0,
                    "Could not determine content type for file \"{0}\". Defaulting to \"application/octet-stream\".",
                    fullPath
                    );
                contentType = "application/octet-stream";
            }
            response.ContentType = contentType;
        }

        string GetETag(string fullPath)
        {
            using (var hash = SHA1.Create())
            {
                using (var file = File.OpenRead(fullPath))
                {
                    return "\"" + Convert.ToBase64String(hash.ComputeHash(file)) + "\"";
                }
            }
        }

        string ContentTypeFromExtension(string extension)
        {
            string contentType;
            if (ContentTypes.TryGetValue(extension, out contentType))
            {
                return contentType;
            }
            return null;
        }
    }
}