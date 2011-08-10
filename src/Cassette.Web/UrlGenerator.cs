using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

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
                HexString(module.Assets[0].Hash)
            );
        }

        public string CreateAssetUrl(Module module, IAsset asset)
        {
            return string.Format(
                "{0}/{1}/{2}/{3}?{4}",
                urlRootPath,
                assetsPrefix,
                ConvertToForwardSlashes(module.Directory),
                ConvertToForwardSlashes(asset.SourceFilename),
                HexString(asset.Hash)
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

        string HexString(byte[] bytes)
        {
            return bytes.Aggregate(
                new StringBuilder(),
                (builder, b) => builder.Append(b.ToString("x2"))
            ).ToString();
        }
    }
}
