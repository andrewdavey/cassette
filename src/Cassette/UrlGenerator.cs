using System;
using Cassette.Utilities;

namespace Cassette
{
    class UrlGenerator : IUrlGenerator
    {
        readonly IUrlModifier urlModifier;
        private readonly IApplicationRootPrepender applicationRootPrepender;

        public UrlGenerator(IUrlModifier urlModifier, IApplicationRootPrepender applicationRootPrepender)
        {
            this.urlModifier = urlModifier;
            this.applicationRootPrepender = applicationRootPrepender;
        }

        public string CreateBundleUrl(Bundle bundle)
        {
            return urlModifier.Modify(bundle.Url);
        }

        public string CreateAssetUrl(IAsset asset)
        {
            // "~/directory/file.js" --> "cassette.axd/asset/directory/file.js?hash"
            // Asset URLs are only used in debug mode. The hash is placed in the querystring, not the path.
            // This maintains the asset directory structure i.e. two assets in the same directory appear together in web browser JavaScript development tooling.
            
            var assetPath = asset.Path.Substring(1);
            var hash = asset.Hash.ToUrlSafeBase64String();
            var url = "asset" + assetPath + "?" + hash;

            return urlModifier.Modify(url);
        }

        public string CreateRawFileUrl(string filename, string hash)
        {
            if (filename.StartsWith("~") == false)
            {
                throw new ArgumentException("Image filename must be application relative (starting with '~').");
            }

            // "~\example\image.png" --> "/example/image-hash.png"
            var path = ConvertToForwardSlashes(filename).Substring(1);
            var index = path.LastIndexOf('.');
            if (index >= 0)
            {
                path = path.Insert(index, "-" + hash);
            }
            else
            {
                path = path + "-" + hash;
            }

            var url = "file" + path;
            return urlModifier.Modify(url);
        }

        public string CreateAbsolutePathUrl(string applicationRelativePath)
        {
            var url = applicationRelativePath.TrimStart('~', '/');
            return applicationRootPrepender.Modify(url);
        }

        string ConvertToForwardSlashes(string path)
        {
            return path.Replace('\\', '/');
        }
    }
}