using Should;
using Xunit;

namespace Cassette.CDN
{
    public class CdnUrlGenerator_CreateAbsolutePathUrl_Tests : CdnUrlGeneratorTestsBase
    {
        [Fact]
        public void ItRemovesTildeSlashPrefix()
        {
            CdnUrlGenerator.CreateAbsolutePathUrl("~/test.png").ShouldEqual(CdnTestUrl + "/test.png");
        }

        [Fact]
        public void ItCallsUrlModifier()
        {
            UrlModifier.Setup(m => m.Modify("test.png")).Returns("/app/test.png");
            CdnUrlGenerator.CreateAbsolutePathUrl("~/test.png").ShouldEqual(CdnTestUrl + "/app/test.png");
        }
    }
}