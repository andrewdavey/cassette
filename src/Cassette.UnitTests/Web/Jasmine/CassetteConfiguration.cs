#if !NET35
using Cassette.Configuration;
using Cassette.Scripts;
using Cassette.Stylesheets;
using Moq;
using Should;
using Xunit;

namespace Cassette.Web.Jasmine
{
    public class CassetteConfiguration_WhenConfigure
    {
        readonly BundleCollection bundles;

        public CassetteConfiguration_WhenConfigure()
        {
            var bundleDefinition = new JasmineBundleDefinition(Mock.Of<IJavaScriptMinifier>(), Mock.Of<IStylesheetMinifier>(), Mock.Of<IUrlGenerator>());
            var settings = new CassetteSettings();
            bundles = new BundleCollection(settings, Mock.Of<IFileSearchProvider>(), Mock.Of<IBundleFactoryProvider>());

            bundleDefinition.AddBundles(bundles);
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