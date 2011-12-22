using Cassette.BundleProcessing;
using Cassette.Configuration;
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
        public void RenderUsesRenderer()
        {
            var bundle = new ExternalStylesheetBundle("http://test.com/asset.css");
            var urlGenerator = new Mock<IUrlGenerator>();
            urlGenerator.Setup(g => g.CreateBundleUrl(bundle)).Returns("/");
            var settings = new CassetteSettings("")
            {
                UrlGenerator = urlGenerator.Object
            };
            bundle.Process(settings);

            var html = bundle.Render();

            html.ShouldContain(bundle.Url);
        }

        [Fact]
        public void GivenMediaNotEmpty_RenderReturnsHtmlLinkElementWithMediaAttribute()
        {
            var bundle = new ExternalStylesheetBundle("http://test.com/asset.css")
            {
                Media = "print"
            };
            var urlGenerator = new Mock<IUrlGenerator>();
            urlGenerator.Setup(g => g.CreateBundleUrl(bundle)).Returns("/");
            var settings = new CassetteSettings("")
            {
                UrlGenerator = urlGenerator.Object
            };
            bundle.Process(settings);

            var html = bundle.Render();

            html.ShouldContain(bundle.Url);
            html.ShouldContain("media=\"print\"");
        }

        [Fact]
        public void ProcessCallsProcessor()
        {
            var bundle = new ExternalStylesheetBundle("http://test.com/asset.css");
            var processor = new Mock<IBundleProcessor<StylesheetBundle>>();
            bundle.Processor = processor.Object;

            bundle.Process(new CassetteSettings(""));

            processor.Verify(p => p.Process(bundle, It.IsAny<CassetteSettings>()));
        }

        [Fact]
        public void GivenBundleHasName_WhenContainsPathUrl_ThenReturnTrue()
        {
            var bundle = new ExternalStylesheetBundle("http://test.com/asset.css", "test");
            bundle.ContainsPath("http://test.com/asset.css").ShouldBeTrue();
        }
    }
}