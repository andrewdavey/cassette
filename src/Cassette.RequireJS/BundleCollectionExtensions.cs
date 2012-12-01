using System;
using Cassette.Scripts;

namespace Cassette.RequireJS
{
    public static class BundleCollectionExtensions
    {
        public static Func<IModuleInitializer> CreateAmdConfiguration { get; set; } 

        public static void InitializeRequireJsModules(
            this BundleCollection bundles,
            string requireJsPath,
            Action<IModuleInitializer> configuration = null
        )
        {
            var amd = CreateAmdConfiguration();
            amd.InitializeModules(bundles, requireJsPath);
            if (configuration != null) configuration(amd);

            var mainBundle = bundles.Get<ScriptBundle>(amd.MainBundlePath);

            amd.AddRequireJsConfigAssetToMainBundle(mainBundle);

            // The config script will depended on the other bundles having been processed
            // so it can build URLs for them. We ensure the main bundle is processed last
            // by moving it to the end of the collection.
            MoveBundleToEnd(mainBundle, bundles);
        }

        static void MoveBundleToEnd(ScriptBundle mainBundle, BundleCollection bundles)
        {
            bundles.Remove(mainBundle);
            bundles.Add(mainBundle);
        }
    }
}
