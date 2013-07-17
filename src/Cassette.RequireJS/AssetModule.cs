using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using Cassette.Utilities;

namespace Cassette.RequireJS
{
    abstract class AssetModule : IAmdModule
    {
        

        protected AssetModule(IAsset asset, Bundle bundle, string baseUrl)
        {
            if (asset == null) throw new ArgumentNullException("asset");
            if (bundle == null) throw new ArgumentNullException("bundle");

            Asset = asset;
            Path = asset.Path;
            Bundle = bundle;
            ModulePath = ConvertAssetPathToModulePath(asset.Path, baseUrl);
            Alias = ConvertAssetPathToAlias(asset.Path);
            ReferencePaths = asset.References.Select(r => r.ToPath);
        }

        public IAsset Asset { get; private set; }
        public string Path { get; private set; }
        public Bundle Bundle { get; private set; }
        public string ModulePath { get; set; }
        public string Alias { get; set; }
        public IEnumerable<string> ReferencePaths { get; private set; }

        public List<string> GetUrls(IUrlGenerator urlGenerator, bool isDebuggingEnabled)
        {
            string path = isDebuggingEnabled
                             ? urlGenerator.CreateAssetUrl(Asset)
                             : urlGenerator.CreateBundleUrl(Bundle) + "?";

            var urls = new List<string> { path };
            var externalBundle = Bundle as IExternalBundle;
            if (externalBundle != null)
            {
                var externalUrl = externalBundle.ExternalUrl;
                if (externalUrl.EndsWith(".js"))
                {
                    externalUrl = externalUrl.Substring(0, externalUrl.Length - 3);
                }
               urls.Insert(0,externalUrl);
            }
            return urls;
        }

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

        internal static string ConvertAssetPathToAlias(string assetPath)
        {
            var name = System.IO.Path.GetFileNameWithoutExtension(assetPath);
            if (!char.IsLetter(name[0]) && name[0] != '_') name = "_" + name;
            var safeName = Regex.Replace(name, "[^a-zA-Z0-9_]", match => "_");
            return safeName;
        }
    }
}