using Cassette.Configuration;
using Cassette.Utilities;
using Moq;
using Xunit;

namespace Cassette.Stylesheets
{
    public class ParseLessReferences_Tests
    {
        [Fact]
        public void ProcessAddsReferencesToLessAssetInBundle()
        {
            var asset = new Mock<IAsset>();
            asset.SetupGet(a => a.SourceFile.FullPath).Returns("~/asset.less");

            var lessSource = @"
// @reference ""another1.less"";
// @reference '/another2.less';
// @reference '../test/another3.less';
";
            asset.Setup(a => a.OpenStream())
                 .Returns(lessSource.AsStream());
            var bundle = new StylesheetBundle("~");
            bundle.Assets.Add(asset.Object);

            var processor = new ParseLessReferences();
            processor.Process(bundle, new CassetteSettings(""));

            asset.Verify(a => a.AddReference("another1.less", 2));
            asset.Verify(a => a.AddReference("/another2.less", 3));
            asset.Verify(a => a.AddReference("../test/another3.less", 4));
        }
    }
}

