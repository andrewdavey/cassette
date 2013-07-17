using System;
using System.IO;
using System.Text.RegularExpressions;
using Cassette.Utilities;

namespace Cassette.RequireJS
{
    abstract class Module : IAmdModule
    {
        protected Module(IAsset asset, Bundle bundle, string baseUrl)
        {
            if (asset == null) throw new ArgumentNullException("asset");
            if (bundle == null) throw new ArgumentNullException("bundle");

            Asset = asset;
            Bundle = bundle;
            ModulePath = ConvertAssetPathToModulePath(asset.Path, baseUrl);
            Alias = ConvertAssetPathToAlias(asset.Path);
        }

        public IAsset Asset { get; private set; }
        public Bundle Bundle { get; private set; }
        public string ModulePath { get; set; }
        public string Alias { get; set; }

        internal static string ConvertAssetPathToModulePath(string assetPath, string baseUrl)
        {
            // "~/foo/bar.js" --> "foo/bar"
            var path = assetPath.TrimStart('~', '/');
            var modulePath  = RemoveFileExtension(path);

            if (!String.IsNullOrEmpty(baseUrl) && modulePath.StartsWith(baseUrl))
            {
                return modulePath.Substring(baseUrl.Length + 1);
            }
            return modulePath;
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