using Cassette.Configuration;
using Cassette.Scripts;

namespace CdnSample
{
    public class CassetteConfiguration : ICassetteConfiguration
    {
        public void Configure(BundleCollection bundles, CassetteSettings settings)
        {
            bundles.AddPerSubDirectory<ScriptBundle>("Scripts");
        }
    }
}