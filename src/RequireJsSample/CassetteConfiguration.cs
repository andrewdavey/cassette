using Cassette;
using Cassette.RequireJS;
using Cassette.Scripts;

namespace RequireJsSample
{
    public class CassetteConfiguration : IConfiguration<BundleCollection>
    {
        public void Configure(BundleCollection bundles)
        {
            bundles.AddPerSubDirectory<ScriptBundle>("Scripts");
        }
    }

    public class RequireJsConfig : IConfiguration<RequireJsSettings>
    {
        public void Configure(RequireJsSettings settings)
        {
            settings.RequireJsPath = "~/require.js";
        }
    }
}