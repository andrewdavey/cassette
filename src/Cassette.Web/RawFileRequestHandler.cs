#region License
/*
Copyright 2011 Andrew Davey

This file is part of Cassette.

Cassette is free software: you can redistribute it and/or modify it under the 
terms of the GNU General Public License as published by the Free Software 
Foundation, either version 3 of the License, or (at your option) any later 
version.

Cassette is distributed in the hope that it will be useful, but WITHOUT ANY 
WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS 
FOR A PARTICULAR PURPOSE. See the GNU General Public License for more details.

You should have received a copy of the GNU General Public License along with 
Cassette. If not, see http://www.gnu.org/licenses/.
*/
#endregion

using System;
using System.Collections.Generic;
using System.IO;
using System.Text.RegularExpressions;
using System.Web;
using System.Web.Routing;
using System.Diagnostics;

namespace Cassette.Web
{
    public class RawFileRequestHandler : IHttpHandler
    {
        public RawFileRequestHandler(RequestContext requestContext)
        {
            routeData = requestContext.RouteData;
            response = requestContext.HttpContext.Response;
            server = requestContext.HttpContext.Server;
        }

        readonly RouteData routeData;
        readonly HttpResponseBase response;
        readonly HttpServerUtilityBase server;

        readonly Dictionary<string, string> contentTypes =
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
                Trace.Source.TraceEvent(TraceEventType.Error, 0, "Invalid file path in URL \"{0}\".", path);
                response.StatusCode = 404;
                return;
            }
            var extension = match.Groups["extension"].Value;

            var filename = match.Groups["filename"].Value + "." + extension;
            var fullPath = server.MapPath("~/" + filename);
            if (File.Exists(fullPath))
            {
                var contentType = ContentTypeFromExtension(extension);
                if (contentType != null)
                {
                    Trace.Source.TraceEvent(TraceEventType.Error, 0, "Sending file \"{0}\" with content type {1}.", fullPath, response.ContentType);
                }
                else
                {
                    Trace.Source.TraceEvent(TraceEventType.Warning, 0, "Could not determine content type for file \"{0}\". Defaulting to \"application/octet-stream\".", fullPath);
                    contentType = "application/octet-stream";
                }
                response.ContentType = contentType;
                response.Cache.SetCacheability(HttpCacheability.Public);
                response.Cache.SetExpires(DateTime.Now.AddYears(1));
                response.WriteFile(fullPath);
            }
            else
            {
                Trace.Source.TraceEvent(TraceEventType.Error, 0, "File not found \"{0}\".", fullPath);
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