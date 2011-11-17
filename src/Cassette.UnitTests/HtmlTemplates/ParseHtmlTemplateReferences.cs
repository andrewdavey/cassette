using Moq;
using Xunit;
using Cassette.Utilities;

namespace Cassette.HtmlTemplates
{
    public class ParseHtmlTemplateReferences_Tests
    {
        [Fact]
        public void ProcessAddsReferencesToJavaScriptAssetInBundle()
        {
            var asset = new Mock<IAsset>();
            asset.SetupGet(a => a.SourceFile.FullPath).Returns("~/asset.js");

            var javaScriptSource = @"
<!-- @reference ""another1.js""
     @reference 'another2.js'
     @reference another3.js another4.js
-->";
            asset.Setup(a => a.OpenStream())
                 .Returns(javaScriptSource.AsStream());
            var bundle = new HtmlTemplateBundle("~");
            bundle.Assets.Add(asset.Object);

            var processor = new ParseHtmlTemplateReferences();
            processor.Process(bundle, Mock.Of<ICassetteApplication>());

            asset.Verify(a => a.AddReference("another1.js", 2));
            asset.Verify(a => a.AddReference("another2.js", 3));
            asset.Verify(a => a.AddReference("another3.js", 4));
            asset.Verify(a => a.AddReference("another4.js", 4));
        }
    }
}