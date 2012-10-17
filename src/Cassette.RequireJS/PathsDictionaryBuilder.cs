using System.Collections.Generic;

namespace Cassette.RequireJS
{
    class PathsDictionaryBuilder : IBundleVisitor
    {
        public static Dictionary<string,string> Build(IEnumerable<Bundle> bundles, IUrlGenerator urlGenerator, bool isDebuggingEnabled)
        {
            var builder = new PathsDictionaryBuilder(urlGenerator, isDebuggingEnabled);
            bundles.Accept(builder);
            return builder.paths;
        }

        readonly IUrlGenerator urlGenerator;
        readonly bool isDebuggingEnabled;
        readonly Dictionary<string, string> paths = new Dictionary<string, string>();
        Bundle currentBundle;

        PathsDictionaryBuilder(IUrlGenerator urlGenerator, bool isDebuggingEnabled)
        {
            this.urlGenerator = urlGenerator;
            this.isDebuggingEnabled = isDebuggingEnabled;
        }

        public void Visit(Bundle bundle)
        {
            currentBundle = bundle;
        }

        public void Visit(IAsset asset)
        {
            if (isDebuggingEnabled)
            {
                AddDebugPathMapping(asset);
            }
            else
            {
                AddReleasePathMapping(asset);
            }
        }

        void AddDebugPathMapping(IAsset asset)
        {
            var path = PathHelpers.ConvertCassettePathToModulePath(asset.Path);
            var url = urlGenerator.CreateAssetUrl(asset);
            paths.Add(path, url);
        }

        void AddReleasePathMapping(IAsset asset)
        {
            var path = PathHelpers.ConvertCassettePathToModulePath(asset.Path);
            var url = urlGenerator.CreateBundleUrl(currentBundle);
            paths.Add(path, url);
        }
    }
}