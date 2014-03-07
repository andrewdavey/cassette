using Cassette.BundleProcessing;
using Cassette.Stylesheets;
using Moq;
using Should;
using Xunit;

namespace Cassette.CDN
{
    public class CdnStylesheetBundle_Tests
    {
        [Fact]
        public void CanCreateNamedBundle()
        {
            var bundle = new CdnStylesheetBundle("http://url.com/", "~/name");
            bundle.Path.ShouldEqual("~/name");
        }

        [Fact]
        public void CreateNamedBundle_ThenPathIsAppRelative()
        {
            var bundle = new CdnStylesheetBundle("http://url.com/", "name");
            bundle.Path.ShouldEqual("~/name");
        }

        [Fact]
        public void CreateWithOnlyUrl_ThenPathIsUrl()
        {
            var bundle = new CdnStylesheetBundle("http://test.com/api.css");
            bundle.Path.ShouldEqual("http://test.com/api.css");
        }

        [Fact]
        public void ProcessCallsProcessor()
        {
            var bundle = new CdnStylesheetBundle("http://test.com/asset.css");
            var pipeline = new Mock<IBundlePipeline<StylesheetBundle>>();
            bundle.Pipeline = pipeline.Object;

            bundle.Process(new CassetteSettings());

            pipeline.Verify(p => p.Process(bundle));
        }

        [Fact]
        public void GivenBundleHasName_WhenContainsPathUrl_ThenReturnTrue()
        {
            var bundle = new CdnStylesheetBundle("http://test.com/asset.css", "test");
            bundle.ContainsPath("http://test.com/asset.css").ShouldBeTrue();
        }

        [Fact]
        public void GivenDifferentUrls_ThenCdnStylesheetBundlesNotEqual()
        {
            var b1 = new CdnStylesheetBundle("http://test1.com/a", "a");
            var b2 = new CdnStylesheetBundle("http://test2.com/a", "a");
            b1.Equals(b2).ShouldBeFalse();
        }
    }
}