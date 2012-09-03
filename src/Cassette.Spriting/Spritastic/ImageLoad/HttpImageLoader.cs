using System;
using Cassette.Spriting.Spritastic.Utilities;

namespace Cassette.Spriting.Spritastic.ImageLoad
{
    class HttpImageLoader : IImageLoader
    {
        protected readonly IWebClientWrapper WebClientWrapper;

        public HttpImageLoader(IWebClientWrapper webClientWrapper)
        {
            WebClientWrapper = webClientWrapper;
        }

        public string BasePath { get; set; }

        public byte[] GetImageBytes(string url)
        {
            return WebClientWrapper.DownloadBytes(new Uri(new Uri(BasePath), url).AbsoluteUri);
        }
    }
}