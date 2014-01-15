using System;
using System.Net;
using System.Web;
using Cassette.IO;
#if NET35
using Cassette.Utilities;
#endif

namespace Cassette.Aspnet
{
    public class CachedFileRequestHandler : ICassetteRequestHandler
    {
		readonly IDirectory cacheDirectory;
		readonly HttpResponseBase response;
		readonly HttpRequestBase request;

        public CachedFileRequestHandler(HttpRequestBase request, HttpResponseBase response, IDirectory cacheDirectory)
        {
			this.cacheDirectory = cacheDirectory;
			this.response = response;
			this.request = request;
        }

        public void ProcessRequest(string path)
        {
            var file = cacheDirectory.GetFile(path);
            if (!file.Exists)
            {
                response.StatusCode = (int) HttpStatusCode.NotFound;
                throw new HttpException((int) HttpStatusCode.NotFound, "File not found");
            }

            SetContentType(path);
            SendFile(file);
        }

        void SetContentType(string path)
        {
            string contentType;
            if (ContentTypes.TryGetContentTypeForFilename(path, out contentType))
            {
                response.ContentType = contentType;
            }
        }

        void SendFile(IFile file)
        {
			HttpResponseUtil.EncodeStreamAndAppendResponseHeaders(request, response);
            response.Cache.SetCacheability(HttpCacheability.Public);
            response.Cache.SetExpires(DateTime.UtcNow.AddYears(1));
            response.Cache.SetMaxAge(new TimeSpan(365, 0, 0, 0));

            using (var stream = file.OpenRead())
            {
                stream.CopyTo(response.OutputStream);
            }
        }
    }
}