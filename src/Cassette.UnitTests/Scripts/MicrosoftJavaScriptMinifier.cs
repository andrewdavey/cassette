using Should;
using Xunit;

namespace Cassette.Scripts
{
    public class MicrosoftJavaScriptMinifier_Tests
    {
        [Fact]
        public void TransformMinifiesJavaScript()
        {
            var minifier = new MicrosoftJavaScriptMinifier();
            var result = minifier.Transform("function () { }", new StubAsset());
            result.ShouldEqual("function(){}");
        }
    }
}