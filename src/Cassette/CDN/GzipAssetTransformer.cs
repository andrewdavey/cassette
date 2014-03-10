using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Text;

namespace Cassette.CDN
{
    public class GzipAssetTransformer : IAssetTransformer
    {
        public Func<Stream> Transform(Func<Stream> openSourceStream, IAsset asset)
        {
            return delegate
            {
                var assetContent = ReadAssetAsBytes(openSourceStream);
                var memoryStream = new MemoryStream();

                using (var gzipStream = new GZipStream(memoryStream, CompressionMode.Compress, true))
                {
                    gzipStream.Write(assetContent, 0, assetContent.Length);
                    gzipStream.Flush();
                }

                memoryStream.Position = 0L;
                return (Stream)memoryStream;
            };
        }

        private byte[] ReadAssetAsBytes(Func<Stream> openSourceStream)
        {
            string assetContent;

            using (var reader = new StreamReader(openSourceStream()))
            {
                assetContent = reader.ReadToEnd();
            }

            return Encoding.UTF8.GetBytes(assetContent);
        }
    }
}
