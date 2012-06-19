using Moq;
using Should;
using Xunit;

namespace Cassette.Views
{
    public class BundlesHelper_Tests
    {
        [Fact]
        public void UrlThrowsUsefulExceptionWhenBundleNotFound()
        {
            var settings = new CassetteSettings();
            var bundles = new BundleCollection(settings, Mock.Of<IFileSearchProvider>(), Mock.Of<IBundleFactoryProvider>());
            var helper = new BundlesHelper(bundles, settings, Mock.Of<IUrlGenerator>(), () => Mock.Of<IReferenceBuilder>(), Mock.Of<IFileAccessAuthorization>(), Mock.Of<IBundleCacheRebuilder>());

            var exception = Record.Exception(
                () => helper.Url<Scripts.ScriptBundle>("~/notfound")
            );

            exception.Message.ShouldEqual("Bundle not found with path \"~/notfound\".");
        }
    }
}