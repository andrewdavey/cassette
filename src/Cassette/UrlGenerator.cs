using System;
using System.Linq;
using Cassette.Utilities;

namespace Cassette
{
    class UrlGenerator : IUrlGenerator
    {
        readonly IUrlModifier urlModifier;
        readonly string routePrefix;

        public UrlGenerator(IUrlModifier urlModifier)
        {
            this.urlModifier = urlModifier;
            routePrefix = "_cassette";
        }

        public string CreateBundleUrl(Bundle bundle)
        {
            var url = routePrefix + "/" + bundle.Url;
            return urlModifier.Modify(url);
        }

        public string CreateAssetUrl(IAsset asset)
        {
            // asset path will be something like:
            // "~/directory/file.js"

            var parts = asset.Path.Split('/');
            // [ "~", "directory", "file.js" ]

            parts = parts.Skip(1).ToArray(); // skip the "~"
            // [ "directory", "file.js" ]

            var filename = parts[parts.Length - 1]; // "file.js"
            var periodIndex = filename.LastIndexOf('.');
            string extension = null;
            if (periodIndex >= 0)
            {
                var filenameWithoutExtension = filename.Substring(0, periodIndex);
                parts[parts.Length - 1] = filenameWithoutExtension;
                extension = filename.Substring(periodIndex + 1);
                // parts == [ "directory", "file" ]
                // extension == "js"
            }
            var path = string.Join("/", parts);
            if (extension != null)
            {
                path += "_" + extension;
            }

            return urlModifier.Modify(
                string.Format(
                    "{0}/asset/{1}?{2}",
                    routePrefix,
                    path,
                    asset.Hash.ToHexString()
                )
            );
        }

        public string CreateRawFileUrl(string filename, string hash)
        {
            if (filename.StartsWith("~") == false)
            {
                throw new ArgumentException("Image filename must be application relative (starting with '~').");
            }

            filename = filename.Substring(2); // Remove the "~/"
            var dotIndex = filename.LastIndexOf('.');
            var name = filename.Substring(0, dotIndex);
            var extension = filename.Substring(dotIndex + 1);

            return urlModifier.Modify(
                string.Format(
                    "{0}/file/{1}_{2}_{3}",
                    routePrefix,
                    ConvertToForwardSlashes(name),
                    hash,
                    extension
                )
            );
        }

        public string CreateAbsolutePathUrl(string applicationRelativePath)
        {
            var url = applicationRelativePath.TrimStart('~', '/');
            return urlModifier.Modify(url);
        }

        string ConvertToForwardSlashes(string path)
        {
            return path.Replace('\\', '/');
        }
    }
}