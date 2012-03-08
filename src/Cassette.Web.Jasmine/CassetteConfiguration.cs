using Cassette.Configuration;
using Cassette.Scripts;
using Cassette.Stylesheets;

namespace Cassette.Web.Jasmine
{
    public class CassetteConfiguration : ICassetteConfiguration
    {
        public void Configure(BundleCollection bundles, CassetteSettings settings)
        {
            var script = new ScriptBundle("cassette.web.jasmine");
            script.Assets.Add(new ResourceAsset("Cassette.Web.Jasmine.jasmine.js", GetType().Assembly));
            bundles.Add(script);

            var css = new StylesheetBundle("cassette.web.jasmine");
            css.Assets.Add(new ResourceAsset("Cassette.Web.Jasmine.jasmine.css", GetType().Assembly));
            bundles.Add(css);
        }
    }
}