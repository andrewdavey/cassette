using System;
using Cassette.Utilities;

namespace Cassette
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
            return urlModifier.Modify(
                String.Format(
                    "{0}/{1}/{2}_{3}",
                    routePrefix,
                    ConventionalBundlePathName(bundle.GetType()),
                    bundle.PathWithoutPrefix,
                    bundle.Hash.ToHexString()
                )
            );
        }

        public string CreateAssetUrl(IAsset asset)
        {
            return urlModifier.Modify(
                String.Format(
                    "{0}/asset/{1}?{2}",
                    routePrefix,
                    asset.SourceFile.FullPath.Substring(2),
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
                String.Format(
                    "{0}/file/{1}_{2}.{3}",
                    routePrefix,
                    ConvertToForwardSlashes(name),
                    hash,
                    extension
                )
            );
        }

        // TODO: move RoutePrefix to settings?
        public const string RoutePrefix = "_cassette";

        public static string ConventionalBundlePathName(Type bundleType)
        {
            // ExternalScriptBundle subclasses ScriptBundle, but we want the name to still be "scripts"
            // So walk up the inheritance chain until we get to something that directly inherits from Bundle.
            while (bundleType != null && bundleType.BaseType != typeof(Bundle))
            {
                bundleType = bundleType.BaseType;
            }
            if (bundleType == null) throw new ArgumentException("Type must be a subclass of Cassette.Bundle.", "bundleType");

            return bundleType.Name.ToLowerInvariant();
        }

        string ConvertToForwardSlashes(string path)
        {
            return path.Replace('\\', '/');
        }
    }
}