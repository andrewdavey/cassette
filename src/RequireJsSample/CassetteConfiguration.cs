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

            amd.ModulePerAsset("~/Scripts/page1");
            amd.ModulePerAsset("~/Scripts/page2");
            amd.AddModule("~/Scripts/app/jquery.js", "$");
            amd.MainBundlePath = "Scripts/app";
        }
    }
}