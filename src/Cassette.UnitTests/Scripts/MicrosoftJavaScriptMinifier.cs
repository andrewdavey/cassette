using Cassette.Utilities;
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
            var getResult = minifier.Transform(() => "function () { }".AsStream(), new StubAsset());
            getResult().ReadToEnd().ShouldEqual("function(){}");
        }
    }
}

