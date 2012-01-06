using Cassette.Configuration;
using Cassette.Scripts;
using Cassette.Stylesheets;

namespace CdnSample
{
    public class CassetteConfiguration : ICassetteConfiguration
    {
        public void Configure(BundleCollection bundles, CassetteSettings settings)
        {
            bundles.AddPerSubDirectory<ScriptBundle>("Scripts");
            bundles.Add<StylesheetBundle>("Content");
        }
    }
}