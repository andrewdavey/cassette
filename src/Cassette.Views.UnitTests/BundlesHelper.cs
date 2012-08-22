using System.Linq;
using Cassette.Scripts;
using Moq;
using Should;
using Xunit;

namespace Cassette.Views
{
    public class BundlesHelper_Tests
    {
        readonly CassetteSettings settings;
        readonly BundleCollection bundles;
        readonly BundlesHelper helper;
        readonly Mock<IReferenceBuilder> referenceBuilder;

        public BundlesHelper_Tests()
        {
            settings = new CassetteSettings();
            bundles = new BundleCollection(settings, Mock.Of<IFileSearchProvider>(), Mock.Of<IBundleFactoryProvider>());
            referenceBuilder = new Mock<IReferenceBuilder>();
            var urlModifier = new VirtualDirectoryPrepender("/");
            var urlGenerator = new UrlGenerator(urlModifier, "cassette.axd/");
            helper = new BundlesHelper(bundles, settings, urlGenerator, () => referenceBuilder.Object, Mock.Of<IFileAccessAuthorization>(), Mock.Of<IBundleCacheRebuilder>(), new SimpleJsonSerializer());
        }

        [Fact]
        public void UrlThrowsUsefulExceptionWhenBundleNotFound()
        {
            var exception = Record.Exception(
                () => helper.Url<Scripts.ScriptBundle>("~/notfound")
            );

            exception.Message.ShouldEqual("Bundle not found with path \"~/notfound\".");
        }

        [Fact]
        public void GetReferencedBundleUrlsReturnExternalUrlForExternalScriptBundle()
        {
            var bundle = new ExternalScriptBundle("http://example.com/", "~/test", "!test");
            bundle.Assets.Add(new StubAsset());

            referenceBuilder.Setup(b => b.GetBundles(null)).Returns(new[] { bundle });
            var urls = helper.GetReferencedBundleUrls<ScriptBundle>(null).ToArray();
            urls.ShouldEqual(new[] { "http://example.com/" });
        }
    }
}