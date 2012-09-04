using System;
using System.IO;
using System.Text.RegularExpressions;
using Cassette.IO;
using Cassette.Spriting.Spritastic.ImageLoad;

namespace Cassette.Spriting
{
    class ImageFileLoader : IImageLoader
    {
        readonly IDirectory directory;
        readonly IUrlGenerator urlGenerator;

        public ImageFileLoader(IDirectory directory, IUrlGenerator urlGenerator)
        {
            this.directory = directory;
            this.urlGenerator = urlGenerator;
        }

        public string BasePath { get; set; }

        public byte[] GetImageBytes(string url)
        {
            using (var stream = OpenFile(url))
            using (var memory = new MemoryStream((int)stream.Length))
            {
                stream.CopyTo(memory);
                return memory.ToArray();
            }
        }

        Stream OpenFile(string url)
        {
            var path = GetFilePathFromCassetteFileUrl(url);
            return directory.GetFile(path).OpenRead();
        }

        string GetFilePathFromCassetteFileUrl(string url)
        {
            // Image URLs will already have been rewritten by the ExpandCssUrls processor.
            // e.g. "/cassette.axd/file/some/path-hash.png"
            // We need to map back to the original path
            // e.g. "some/path.png"

            var urlRegex = UrlRegex();
            var match = urlRegex.Match(url);
            if (!match.Success) throw new ArgumentException("URL must be a Cassette URL of a PNG image.", "url");

            var filename = match.Groups["filename"].Value;
            return filename + ".png";
        }

        Regex UrlRegex()
        {
            var prefix = Regex.Escape(RawFileUrlPrefix());
            return new Regex(
                prefix + @"(?<filename>.*)-[a-z0-9]+\.png",
                RegexOptions.IgnoreCase
            );
        }

        string RawFileUrlPrefix()
        {
            var dummyUrl = urlGenerator.CreateRawFileUrl("~/dummy", "hash");
            var prefixLength = dummyUrl.IndexOf("dummy", StringComparison.Ordinal);
            return dummyUrl.Substring(0, prefixLength);
        }
    }
}