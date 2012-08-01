#if !NET35
using Cassette.BundleProcessing;
using Cassette.Scripts;
using Cassette.Stylesheets;
using Moq;
using Should;
using Xunit;

namespace Cassette.Aspnet.Jasmine
{
    public class JasmineBundleConfiguration_WhenConfigure
    {
        readonly BundleCollection bundles;

        public JasmineBundleConfiguration_WhenConfigure()
        {
            var scriptBundleFactory = new ScriptBundleFactory(() => Mock.Of<IBundlePipeline<ScriptBundle>>());
            var stylesheetBundleFactory = new StylesheetBundleFactory(() => Mock.Of<IBundlePipeline<StylesheetBundle>>());
            var configuration = new JasmineBundleConfiguration(scriptBundleFactory, stylesheetBundleFactory);
            var settings = new CassetteSettings();
            bundles = new BundleCollection(settings, Mock.Of<IFileSearchProvider>(), Mock.Of<IBundleFactoryProvider>());

            configuration.Configure(bundles);
        }

        [Fact]
        public void ThenScriptBundleAddedToBundles()
        {
            bundles.Get<ScriptBundle>("cassette.aspnet.jasmine").ShouldNotBeNull();
        }

        [Fact]
        public void ThenStylesheetBundleAddedToBundles()
        {
            bundles.Get<StylesheetBundle>("cassette.aspnet.jasmine").ShouldNotBeNull();
        }
    }
}
#endif