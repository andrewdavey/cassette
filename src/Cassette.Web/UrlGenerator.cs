using System;
using Cassette.Utilities;

namespace Cassette.Web
{
    public class UrlGenerator : IUrlGenerator
    {
        public UrlGenerator(string virtualDirectory)
        {
            this.urlRootPath = virtualDirectory.TrimEnd('/');
        }

        readonly string urlRootPath;
        readonly string assetsPrefix = "_assets";

        public string ModuleUrlPattern<T>()
        {
            return string.Format(
                "{0}/{1}/{{*path}}",
                assetsPrefix,
                ConventionalModulePathName(typeof(T))
            );
        }

        public string GetAssetRouteUrl()
        {
            return assetsPrefix + "/get/{*path}";
        }

        public string CreateModuleUrl(Module module)
        {
            return string.Format("{0}/{1}/{2}/{3}_{4}",
                urlRootPath,
                assetsPrefix,
                ConventionalModulePathName(module.GetType()),
                ConvertToForwardSlashes(module.Directory),
                module.Assets[0].Hash.ToHexString()
            );
        }

        public string CreateAssetUrl(Module module, IAsset asset)
        {
            return string.Format(
                "{0}/{1}/{2}?{3}",
                urlRootPath,
                ConvertToForwardSlashes(module.Directory),
                ConvertToForwardSlashes(asset.SourceFilename),
                asset.Hash.ToHexString()
            );
        }

        public string CreateAssetCompileUrl(Module module, IAsset asset)
        {
            return string.Format(
                "{0}/{1}/get/{2}/{3}?{4}",
                urlRootPath,
                assetsPrefix,
                ConvertToForwardSlashes(module.Directory),
                ConvertToForwardSlashes(asset.SourceFilename),
                asset.Hash.ToHexString()
            );
        }

        public string CreateImageUrl(string filename)
        {
            return string.Format("{0}/{1}/images/{2}",
                urlRootPath,
                assetsPrefix,
                ConvertToForwardSlashes(filename)
            );
        }

        string ConventionalModulePathName(Type moduleType)
        {
            var name = moduleType.Name;
            name = name.Substring(0, name.Length - "Module".Length);
            return name.ToLowerInvariant() + "s";
        }

        string ConvertToForwardSlashes(string path)
        {
            return path.Replace('\\', '/');
        }
    }
}