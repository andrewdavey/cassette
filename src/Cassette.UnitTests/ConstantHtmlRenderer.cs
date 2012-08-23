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
            var html = "\"_cassette/some/url1\" \"_cassette/some/url2\" other stuff";
            var renderer = new ConstantHtmlRenderer<TestableBundle>(html, urlModifier);

            var output = renderer.Render(new TestableBundle("~"));

            output.ShouldEqual("\"/root/_cassette/some/url1\" \"/root/_cassette/some/url2\" other stuff");
        }

        static IUrlModifier UrlModifierThatPrependsRoot()
        {
            var urlModifier = new Mock<IUrlModifier>();
            urlModifier
                .Setup(m => m.PostCacheModify(It.IsAny<string>()))
                .Returns<string>(s => "/root/" + s);
            return urlModifier.Object;
        }
    }
}