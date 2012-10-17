using System;
using System.Linq;
using Cassette.Scripts;

namespace Cassette.RequireJS
{
    public class BundlesConfiguration : IConfiguration<BundleCollection>
    {
        readonly RequireJsSettings settings;
        readonly IConfigurationScriptBuilder configurationScriptBuilder;
        const string RequireJsBundlePath = "~/Cassette.RequireJS";

        public BundlesConfiguration(RequireJsSettings settings, IConfigurationScriptBuilder configurationScriptBuilder)
        {
            this.settings = settings;
            this.configurationScriptBuilder = configurationScriptBuilder;
        }

        public void Configure(BundleCollection bundles)
        {
            AddRequireJsBundle(bundles);
        }

        void AddRequireJsBundle(BundleCollection bundles)
        {
            bundles.Add<ScriptBundle>(RequireJsBundlePath, settings.RequireJsPath);

            var bundle = bundles[RequireJsBundlePath];
            bundle.Assets.Insert(0, ConfigScriptAsset(bundles));
        }

        StringAsset ConfigScriptAsset(BundleCollection bundles)
        {
            var path = RequireJsBundlePath + "/config.js";
            var lazyContent = CreateLazyConfigScript(bundles);
            return new StringAsset(path, lazyContent);
        }

        Lazy<string> CreateLazyConfigScript(BundleCollection bundles)
        {
            Lazy<string> lazyContent = null;
            Action reset = () => lazyContent = new Lazy<string>(
                () => configurationScriptBuilder.BuildConfigurationScript(
                    bundles
                        .OfType<ScriptBundle>()
                        .Where(b => b.Path != RequireJsBundlePath)
                        .ToArray()
                )
            );
            reset();
            bundles.Changed += (s, e) => reset();
            return lazyContent;
        }
    }
}