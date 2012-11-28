using System;
using System.Web;
using Cassette.IO;
#if NET35
using Cassette.Utilities;
#endif

namespace Cassette.Aspnet
{
    public class CachedFileRequestHandler : ICassetteRequestHandler
    {
        readonly HttpResponseBase response;
        readonly IDirectory cacheDirectory;

        public CachedFileRequestHandler(HttpResponseBase response, IDirectory cacheDirectory)
        {
            this.response = response;
            this.cacheDirectory = cacheDirectory;
        }

        public void ProcessRequest(string path)
        {
            var file = cacheDirectory.GetFile(path);
            if (!file.Exists)
            {
                response.StatusCode = 404;
                return;
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
            using (var stream = file.OpenRead())
            {
                response.Cache.SetCacheability(HttpCacheability.Public);
                response.Cache.SetExpires(DateTime.UtcNow.AddYears(1));
                response.Cache.SetMaxAge(new TimeSpan(365, 0, 0, 0));
                stream.CopyTo(response.OutputStream);
            }
        }
    }
}