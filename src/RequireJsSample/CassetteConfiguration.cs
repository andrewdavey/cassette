using Cassette;
using Cassette.RequireJS;
using Cassette.Scripts;

namespace RequireJsSample
{
    public class CassetteConfiguration : IConfiguration<BundleCollection>
    {
        readonly AmdConfiguration amd;

        public CassetteConfiguration(AmdConfiguration amd)
        {
            this.amd = amd;
        }

        public void Configure(BundleCollection bundles)
        {
            bundles.AddPerSubDirectory<ScriptBundle>("Scripts");

            amd.InitializeModulesFromBundles(bundles, "Scripts/app/require.js");
            amd.SetImportAlias("Scripts/app/jquery.js", "$");
        }
    }
}