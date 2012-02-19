using Cassette.Configuration;
using Cassette.Scripts;
using Cassette.Stylesheets;

namespace Cassette.Web.Jasmine
{
    public class CassetteConfiguration : ICassetteConfiguration
    {
        public void Configure(BundleCollection bundles, CassetteSettings settings)
        {
            bundles.Add<ScriptBundle>("scripts/jasmine");
            bundles.Add<StylesheetBundle>("scripts/jasmine");
        }
    }
}