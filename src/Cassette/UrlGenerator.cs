using System;
using Cassette.Utilities;

namespace Cassette
{
    class UrlGenerator : IUrlGenerator
    {
        readonly IUrlModifier urlModifier;
        readonly string handlerPath;

        public UrlGenerator(IUrlModifier urlModifier)
        {
            this.urlModifier = urlModifier;
            handlerPath = "cassette.axd";
        }

        public string CreateBundleUrl(Bundle bundle)
        {
            var url = handlerPath + "/" + bundle.Url;
            return urlModifier.Modify(url);
        }

        public string CreateAssetUrl(IAsset asset)
        {
            // "~/directory/file.js" --> "cassette.axd/asset/hash/directory/file.js"
            var url = handlerPath + 
                      "/asset/" + 
                      asset.Hash.ToHexString() + 
                      asset.Path.Substring(1);

            return urlModifier.Modify(url);
        }

        public string CreateRawFileUrl(string filename, string hash)
        {
            if (filename.StartsWith("~") == false)
            {
                throw new ArgumentException("Image filename must be application relative (starting with '~').");
            }

            var url = handlerPath +
                      "/file/" +
                      hash +
                      ConvertToForwardSlashes(filename).Substring(1);

            return urlModifier.Modify(url);
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