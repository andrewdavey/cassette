using Cassette.Configuration;
using Cassette.Scripts;
using Cassette.Stylesheets;
using Should;
using Xunit;

#if NET40
namespace Cassette.Web.Jasmine
{
    public class CassetteConfiguration_WhenConfigure
    {
        readonly BundleCollection bundles;

        public CassetteConfiguration_WhenConfigure()
        {
            var config = new CassetteConfiguration();
            var settings = new CassetteSettings("");
            bundles = new BundleCollection(settings);

            config.Configure(bundles, settings);
        }

        [Fact]
        public void ThenScriptBundleAddedToBundles()
        {
            bundles.Get<ScriptBundle>("cassette.web.jasmine").ShouldNotBeNull();
        }

        [Fact]
        public void ThenStylesheetBundleAddedToBundles()
        {
            bundles.Get<StylesheetBundle>("cassette.web.jasmine").ShouldNotBeNull();
        }
    }
}
#endif