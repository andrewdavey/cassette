using System;
using Cassette.Utilities;

namespace Cassette.Web
{
    class UrlGenerator : IUrlGenerator
    {
        readonly IUrlModifier urlModifier;
        readonly string routePrefix;

        public UrlGenerator(IUrlModifier urlModifier, string routePrefix)
        {
            this.urlModifier = urlModifier;
            this.routePrefix = routePrefix;
        }

        public string CreateBundleUrl(Bundle bundle)
        {
            return urlModifier.Modify(string.Format("{0}/{1}/{2}_{3}",
                                                    routePrefix,
                                                    RoutingHelpers.ConventionalBundlePathName(bundle.GetType()),
                                                    bundle.PathWithoutPrefix,
                                                    bundle.Hash.ToHexString()
                                          ));
        }

        public string CreateAssetUrl(IAsset asset)
        {
            return urlModifier.Modify(string.Format(
                "{0}/asset/{1}?{2}",
                routePrefix,
                asset.SourceFile.FullPath.Substring(2),
                asset.Hash.ToHexString()
                                          ));
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
                    "{0}/file/{1}_{2}.{3}",
                    routePrefix,
                    ConvertToForwardSlashes(name),
                    hash,
                    extension
                )
            );
        }

        string ConvertToForwardSlashes(string path)
        {
            return path.Replace('\\', '/');
        }
    }
}