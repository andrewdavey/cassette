using System;
using System.Collections;
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
        readonly IJsonSerializer jsonSerializer;
        readonly Dictionary<string, IAmdModule> modules = new Dictionary<string, IAmdModule>();

        public AmdConfiguration(BundleCollection bundles, IJsonSerializer jsonSerializer)
        {
            this.bundles = bundles;
            this.jsonSerializer = jsonSerializer;
        }

        public string MainBundlePath { get; set; }

        public void ModulePerAsset(string bundlePath)
        {
            var bundle = bundles.Get<ScriptBundle>(bundlePath);
            foreach (var asset in bundle.Assets)
            {
                var module = new AutoAmdModule(asset, bundle, jsonSerializer, this);
                modules[asset.Path] = module;
                asset.AddAssetTransformer(module);
            }
        }

        public void AddModule(string scriptPath, string alias)
        {
            IAsset asset;
            Bundle bundle;
            if (!bundles.TryGetAssetByPath(scriptPath, out asset, out bundle))
            {
                throw new ArgumentException("Script not found: " + scriptPath);
            }

            var module = new AmdModule(asset, bundle, alias);
            modules[asset.Path] = module;
            asset.AddAssetTransformer(module);
        }

        public void AddModuleUsingShim(string scriptPath, string moduleReturnExpression, params string[] dependencies)
        {
            IAsset asset;
            Bundle bundle;
            if (!bundles.TryGetAssetByPath(scriptPath, out asset, out bundle))
            {
                throw new ArgumentException("Script not found: " + scriptPath);
            }

            var module = new ShimAmdModule(asset, bundle, moduleReturnExpression, dependencies, jsonSerializer);
            modules[asset.Path] = module;
            asset.AddAssetTransformer(module);
        }

        public IAmdModule this[string path]
        {
            get
            {
                IAmdModule module;
                if (modules.TryGetValue(path, out module)) return module;
                throw new ArgumentException("Module not found: " + path);
            }
        }

        public IEnumerator<IAmdModule> GetEnumerator()
        {
            return modules.Values.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        public void AddAutoModule(string scriptPath)
        {
            IAsset asset;
            Bundle bundle;
            if (!bundles.TryGetAssetByPath(scriptPath, out asset, out bundle))
            {
                throw new ArgumentException("Script not found: " + scriptPath);
            }

            var module = new AutoAmdModule(asset, bundle, jsonSerializer, this);
            asset.AddAssetTransformer(module);
            modules[asset.Path] = module;
        }
    }
}