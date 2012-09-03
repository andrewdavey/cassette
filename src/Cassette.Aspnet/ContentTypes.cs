using System.Collections.Generic;

namespace Cassette.Aspnet
{
    internal static class ContentTypes
    {
        static readonly Dictionary<string, string> TypesByExtension = new Dictionary<string, string>
        {
            { "png", "image/png" }
        };

        public static bool TryGetContentTypeForFilename(string filename, out string mimeType)
        {
            var index = filename.LastIndexOf('.');
            if (index < 0)
            {
                mimeType = null;
                return false;
            }
            else
            {
                var extension = filename.Substring(index + 1);
                return TypesByExtension.TryGetValue(extension, out mimeType);
            }
        }
    }
}