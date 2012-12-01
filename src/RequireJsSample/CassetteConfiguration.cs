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

            bundles.InitializeRequireJsModules(
                "Scripts/app/require.js",
                amd =>
                {
                    amd.SetImportAlias("Scripts/app/jquery.js", "$");
                }
            );
        }
    }
}