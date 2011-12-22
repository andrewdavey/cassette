using Cassette.Utilities;
using Moq;
using Xunit;
using Should;

namespace Cassette.Scripts
{
    public class MicrosoftJavaScriptMinifier_Tests
    {
        [Fact]
        public void TransformMinifiesJavaScript()
        {
            var minifier = new MicrosoftJavaScriptMinifier();
            var getResult = minifier.Transform(() => "function () { }".AsStream(), Mock.Of<IAsset>());
            getResult().ReadToEnd().ShouldEqual("function(){}");
        }
    }
}

