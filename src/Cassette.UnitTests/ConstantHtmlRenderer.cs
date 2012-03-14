using Moq;
using Should;
using Xunit;

namespace Cassette
{
    public class ConstantHtmlRenderer_Tests
    {
        [Fact]
        public void RenderModifiesUrlsWrappedWithPlaceholder()
        {
            var urlModifier = UrlModifierThatPrependsRoot();
            var html = "<CASSETTE_URL_ROOT>some/url1</CASSETTE_URL_ROOT> <CASSETTE_URL_ROOT>some/url2</CASSETTE_URL_ROOT> other stuff";
            var renderer = new ConstantHtmlRenderer<TestableBundle>(html, urlModifier);

            var output = renderer.Render(new TestableBundle("~"));

            output.ShouldEqual("/root/some/url1 /root/some/url2 other stuff");
        }

        static IUrlModifier UrlModifierThatPrependsRoot()
        {
            var urlModifier = new Mock<IUrlModifier>();
            urlModifier
                .Setup(m => m.Modify(It.IsAny<string>()))
                .Returns<string>(s => "/root/" + s);
            return urlModifier.Object;
        }
    }
}