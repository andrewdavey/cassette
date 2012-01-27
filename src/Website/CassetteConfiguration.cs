using Cassette.Configuration;
using Cassette.Stylesheets;
using Cassette.Scripts;

namespace Website
{
    public class CassetteConfiguration : ICassetteConfiguration
    {
        public void Configure(BundleCollection bundles, CassetteSettings settings)
        {
            bundles.Add<StylesheetBundle>("assets/styles");
            bundles.Add<StylesheetBundle>("assets/iestyles", b => b.Condition = "IE");
            bundles.AddUrlWithAlias("//ajax.googleapis.com/ajax/libs/jquery/1.6.3/jquery.min.js", "jquery");
            bundles.AddPerSubDirectory<ScriptBundle>("assets/scripts");
        }
    }
}