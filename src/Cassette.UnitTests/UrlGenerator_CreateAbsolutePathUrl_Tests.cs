using Should;
using Xunit;

namespace Cassette
{
    public class UrlGenerator_CreateAbsolutePathUrl_Tests : UrlGeneratorTestsBase
    {
        [Fact]
        public void ItRemovesTildeSlashPrefix()
        {
            UrlGenerator.CreateAbsolutePathUrl("~/test.png").ShouldEqual("test.png");
        }

        [Fact]
        public void ItCallsUrlModifier()
        {
            UrlModifier.Setup(m => m.Modify("test.png")).Returns("/app/test.png");
            UrlGenerator.CreateAbsolutePathUrl("~/test.png").ShouldEqual("/app/test.png");
        }
    }
}