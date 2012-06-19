using Cassette.BundleProcessing;
using Moq;
using Should;
using Xunit;

namespace Cassette.Stylesheets
{
    public class ExternalStylesheetBundle_Tests
    {
        [Fact]
        public void CanCreateNamedBundle()
        {
            var bundle = new ExternalStylesheetBundle("http://url.com/", "~/name");
            bundle.Path.ShouldEqual("~/name");
        }

        [Fact]
        public void CreateNamedBundle_ThenPathIsAppRelative()
        {
            var bundle = new ExternalStylesheetBundle("http://url.com/", "name");
            bundle.Path.ShouldEqual("~/name");
        }

        [Fact]
        public void CreateWithOnlyUrl_ThenPathIsUrl()
        {
            var bundle = new ExternalStylesheetBundle("http://test.com/api.css");
            bundle.Path.ShouldEqual("http://test.com/api.css");
        }

        [Fact]
        public void ProcessCallsProcessor()
        {
            var bundle = new ExternalStylesheetBundle("http://test.com/asset.css");
            var pipeline = new Mock<IBundlePipeline<StylesheetBundle>>();
            bundle.Pipeline = pipeline.Object;

            bundle.Process(new CassetteSettings());

            pipeline.Verify(p => p.Process(bundle));
        }

        [Fact]
        public void GivenBundleHasName_WhenContainsPathUrl_ThenReturnTrue()
        {
            var bundle = new ExternalStylesheetBundle("http://test.com/asset.css", "test");
            bundle.ContainsPath("http://test.com/asset.css").ShouldBeTrue();
        }

        [Fact]
        public void GivenDifferentUrls_ThenExternalStylesheetBundlesNotEqual()
        {
            var b1 = new ExternalStylesheetBundle("http://test1.com/a", "a");
            var b2 = new ExternalStylesheetBundle("http://test2.com/a", "a");
            b1.Equals(b2).ShouldBeFalse();
        }
    }
}