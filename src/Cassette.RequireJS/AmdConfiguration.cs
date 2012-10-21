using System;
using System.Collections.Generic;
using Cassette.Scripts;

namespace Cassette.RequireJS
{
    /// <summary>
    /// Configures script bundles to work as AMD modules.
    /// </summary>
    public class AmdConfiguration : IAmdModuleCollection
    {
        readonly BundleCollection bundles;
        readonly Func<IAssetTransformer> createDefineCallTransformer;
        readonly Func<IAssetTransformer> createModulePathInserter;
        readonly Func<Shim, IAssetTransformer> createModuleShimmer;
        readonly Dictionary<string, AmdModule> modules = new Dictionary<string, AmdModule>();

        public AmdConfiguration(BundleCollection bundles, Func<IAssetTransformer> createDefineCallTransformer, Func<IAssetTransformer> createModulePathInserter, Func<Shim, IAssetTransformer> createModuleShimmer)
        {
            this.bundles = bundles;
            this.createDefineCallTransformer = createDefineCallTransformer;
            this.createModulePathInserter = createModulePathInserter;
            this.createModuleShimmer = createModuleShimmer;
        }

        public string MainBundlePath { get; set; }

        public void ModulePerAsset(string bundlePath)
        {
            var bundle = bundles.Get<ScriptBundle>(bundlePath);
            foreach (var asset in bundle.Assets)
            {
                asset.AddAssetTransformer(createDefineCallTransformer());
                modules[asset.Path] = new AmdModule(asset, bundle);
            }
        }

        public void AddModule(string scriptPath, string alias = null)
        {
            IAsset asset;
            Bundle bundle;
            if (!bundles.TryGetAssetByPath(scriptPath, out asset, out bundle))
            {
                throw new ArgumentException("Script not found: " + scriptPath);
            }

            asset.AddAssetTransformer(createModulePathInserter());

            modules[scriptPath] = new AmdModule(asset, bundle)
            {
                Alias = alias
            };
        }

        public void AddModuleUsingShim(string scriptPath, Shim shim)
        {
            IAsset asset;
            Bundle bundle;
            if (!bundles.TryGetAssetByPath(scriptPath, out asset, out bundle))
            {
                throw new ArgumentException("Script not found: " + scriptPath);
            }

            asset.AddAssetTransformer(createModuleShimmer(shim));

            modules[scriptPath] = new AmdModule(asset, bundle)
            {
                Alias = shim.Alias
            };
        }

        public AmdModule this[string path]
        {
            get { return modules[path]; }
        }
    }
}