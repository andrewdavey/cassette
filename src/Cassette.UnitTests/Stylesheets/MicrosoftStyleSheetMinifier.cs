using Should;
using Xunit;

namespace Cassette.Stylesheets
{
    public class MicrosoftStyleSheetMinifier_Tests
    {
        [Fact]
        public void TransformMinifiesCss()
        {
            var minifier = new MicrosoftStylesheetMinifier();
            var result = minifier.Transform("p { color: White; }", new StubAsset());
            result.ShouldEqual("p{color:#fff}");
        }
    }
}