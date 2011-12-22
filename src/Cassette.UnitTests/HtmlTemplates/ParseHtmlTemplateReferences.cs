using Cassette.Configuration;
using Moq;
using Xunit;
using Cassette.Utilities;

namespace Cassette.HtmlTemplates
{
    public class ParseHtmlTemplateReferences_Tests
    {
        [Fact]
        public void GivenHtmAsset_WhenProcess_ThenItAddsReferencesToHtmlTemplateAssetInBundle()
        {
            var asset = new Mock<IAsset>();
            asset.SetupGet(a => a.SourceFile.FullPath).Returns("~/asset.htm");

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
            processor.Process(bundle, new CassetteSettings(""));

            asset.Verify(a => a.AddReference("another1.js", 2));
            asset.Verify(a => a.AddReference("another2.js", 3));
            asset.Verify(a => a.AddReference("another3.js", 4));
            asset.Verify(a => a.AddReference("another4.js", 4));
        }

        [Fact]
        public void GivenHtmlAsset_WhenProcess_ThenItAddsReferencesToHtmlTemplateAssetInBundle()
        {
            var asset = new Mock<IAsset>();
            asset.SetupGet(a => a.SourceFile.FullPath).Returns("~/asset.html");

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
            processor.Process(bundle, new CassetteSettings(""));

            asset.Verify(a => a.AddReference("another1.js", 2));
            asset.Verify(a => a.AddReference("another2.js", 3));
            asset.Verify(a => a.AddReference("another3.js", 4));
            asset.Verify(a => a.AddReference("another4.js", 4));
        }
    }
}
