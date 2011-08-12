using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Cassette.Utilities;

namespace Cassette.Web
{
    public class UrlGenerator
    {
        public UrlGenerator(string virtualDirectory)
        {
            this.urlRootPath = virtualDirectory.TrimEnd('/');
        }

        readonly string urlRootPath;
        readonly string assetsPrefix = "_assets";

        public string ModuleUrlPattern<T>()
        {
            return string.Format("{0}/{1}/{{*path}}", assetsPrefix, ConvertionalModulePathName(typeof(T)));
        }

        public string CreateModuleUrl(Module module)
        {
            return string.Format("{0}/{1}/{2}/{3}_{4}",
                urlRootPath,
                assetsPrefix,
                ConvertionalModulePathName(module.GetType()),
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

        string ConvertionalModulePathName(Type moduleType)
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
