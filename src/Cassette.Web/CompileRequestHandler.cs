using System;
using System.Collections.Generic;
using System.IO;
using System.Web;
using System.Web.Routing;

namespace Cassette.Web
{
    public class CompileRequestHandler : IHttpHandler
    {
        public CompileRequestHandler(RequestContext requestContext, Func<string, IAsset> getAssetForPath, IDictionary<string, string> contentTypeForFileExtension)
        {
            this.requestContext = requestContext;
            this.getAssetForPath = getAssetForPath;
            this.contentTypeForFileExtension = contentTypeForFileExtension;
        }

        readonly RequestContext requestContext;
        readonly Func<string, IAsset> getAssetForPath;
        readonly IDictionary<string, string> contentTypeForFileExtension;

        public bool IsReusable
        {
            get { return false; }
        }

        public void ProcessRequest(HttpContext _)
        {
            var path = requestContext.RouteData.GetRequiredString("path");
            var response = requestContext.HttpContext.Response;
            var asset = getAssetForPath(path);
            if (asset == null)
            {
                response.StatusCode = 404;
                response.End();
                return;
            }

            response.ContentType = GetContentType(path);
            using (var stream = asset.OpenStream())
            {
                stream.CopyTo(response.OutputStream);
            }
        }

        string GetContentType(string path)
        {
            return contentTypeForFileExtension[Path.GetExtension(path).Substring(1)];
        }
    }
}
