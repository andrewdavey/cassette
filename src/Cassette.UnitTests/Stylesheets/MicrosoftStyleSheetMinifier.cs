using Cassette.Utilities;
using Moq;
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
            var getResult = minifier.Transform(() => "p { color: White; }".AsStream(), Mock.Of<IAsset>());
            using (var result = getResult())
            {
                result.ReadToEnd().ShouldEqual("p{color:#fff}");
            }
        }
    }
}
