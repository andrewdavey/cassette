using System;
using System.IO;
using System.Text.RegularExpressions;

namespace Cassette.RequireJS
{
    abstract class Module : IAmdModule
    {
        protected Module(IAsset asset, Bundle bundle)
        {
            if (asset == null) throw new ArgumentNullException("asset");
            if (bundle == null) throw new ArgumentNullException("bundle");

            Asset = asset;
            Bundle = bundle;
            ModulePath = ConvertAssetPathToModulePath(asset.Path);
            Alias = ConvertAssetPathToAlias(asset.Path);
        }

        public IAsset Asset { get; private set; }
        public Bundle Bundle { get; private set; }
        public string ModulePath { get; set; }
        public string Alias { get; set; }

        static string ConvertAssetPathToModulePath(string assetPath)
        {
            // "~/foo/bar.js" --> "foo/bar"
            var path = assetPath.TrimStart('~', '/');
            return RemoveFileExtension(path);
        }

        static string RemoveFileExtension(string path)
        {
            var index = path.LastIndexOf('.');
            var slash = path.LastIndexOf('/');
            if (index >= 0 && index > slash)
            {
                return path.Substring(0, index);
            }
            return path;
        }

        static string ConvertAssetPathToAlias(string assetPath)
        {
            var name = Path.GetFileNameWithoutExtension(assetPath);
            if (!char.IsLetter(name[0]) && name[0] != '_') name = "_" + name;
            var safeName = Regex.Replace(name, "[^a-zA-Z0-9_]", match => "_");
            return safeName;
        }
    }
}