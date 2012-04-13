#if !NET35
using Cassette.Configuration;
using Cassette.Scripts;
using Cassette.Stylesheets;
using Moq;
using Should;
using Xunit;

namespace Cassette.Web.Jasmine
{
    public class JasmineBundleConfiguration_WhenConfigure
    {
        readonly BundleCollection bundles;

        public JasmineBundleConfiguration_WhenConfigure()
        {
            var configuration = new JasmineBundleConfiguration(Mock.Of<IJavaScriptMinifier>(), Mock.Of<IStylesheetMinifier>(), Mock.Of<IUrlGenerator>());
            var settings = new CassetteSettings();
            bundles = new BundleCollection(settings, Mock.Of<IFileSearchProvider>(), Mock.Of<IBundleFactoryProvider>());

            configuration.Configure(bundles);
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