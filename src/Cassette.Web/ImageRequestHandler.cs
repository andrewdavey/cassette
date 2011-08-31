using System;
using System.Collections.Generic;
using System.IO;
using System.Text.RegularExpressions;
using System.Web;
using System.Web.Routing;

namespace Cassette.Web
{
    public class ImageRequestHandler : IHttpHandler
    {
        public ImageRequestHandler(RequestContext requestContext)
        {
            routeData = requestContext.RouteData;
            response = requestContext.HttpContext.Response;
            server = requestContext.HttpContext.Server;
        }

        readonly RouteData routeData;
        readonly HttpResponseBase response;
        readonly HttpServerUtilityBase server;
        readonly Dictionary<string, string> contentTypes = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)
        {
            { "png", "image/png"},
            { "jpg", "image/jpeg"},
            { "jpeg", "image/jpeg"},
            { "gif", "image/gif"}
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
                response.StatusCode = 404;
                return;
            }
            var extension = match.Groups["extension"].Value;

            var filename = match.Groups["filename"].Value + "." + extension;
            var fullPath = server.MapPath("~/" + filename);
            if (File.Exists(fullPath))
            {
                response.ContentType = ContentTypeFromExtension(extension);
                response.Cache.SetCacheability(HttpCacheability.Public);
                response.Cache.SetExpires(DateTime.Now.AddYears(1));
                // TODO: handle content type not found (null).
                response.WriteFile(fullPath);
            }
            else
            {
                response.StatusCode = 404;
            }
        }

        string ContentTypeFromExtension(string extension)
        {
            string contentType;
            if (contentTypes.TryGetValue(extension, out contentType))
            {
                return contentType;
            }
            return null;
        }
    }
}
