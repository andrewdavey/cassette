using Cassette.Configuration;
using Cassette.Scripts;
using Cassette.Stylesheets;

namespace Precompiled
{
    public class CassetteConfiguration : ICassetteConfiguration
    {
        public void Configure(BundleCollection bundles, CassetteSettings settings)
        {
            // This configuration is first used at compile time to generate a manifest of all bundles.

            // Then at runtime it is called again, but this time settings.IsUsingPrecompiledManifest == true.
            // So we can skip adding the bundles. They will be added automatically from the manifest.
            if (settings.IsUsingPrecompiledManifest) return;

            bundles.Add<StylesheetBundle>("Content");
            bundles.Add<ScriptBundle>("Scripts");
        }
    }
}