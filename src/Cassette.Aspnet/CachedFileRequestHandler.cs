using System.Web;
using Cassette.IO;

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

            using(var stream = file.OpenRead())
            {
                response.ContentType = "image/png";
                stream.CopyTo(response.OutputStream);
            }
        }
    }
}