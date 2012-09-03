using System.IO;
using System.Text.RegularExpressions;
using Cassette.IO;
using Cassette.Spriting.Spritastic.ImageLoad;

namespace Cassette.Spriting
{
    class ImageFileLoader : IImageLoader
    {
        readonly IDirectory directory;

        public ImageFileLoader(IDirectory directory)
        {
            this.directory = directory;
        }

        public string BasePath { get; set; }

        public byte[] GetImageBytes(string url)
        {
            var match = Regex.Match(url, @"/cassette\.axd/file/(.*)-[a-z0-9]+\.png$");
            var path = match.Groups[1].Value + ".png";

            var file = directory.GetFile(path);
            using (var stream = file.OpenRead())
            using (var memory = new MemoryStream((int)stream.Length))
            {
                stream.CopyTo(memory);
                return memory.ToArray();
            }
        }
    }
}