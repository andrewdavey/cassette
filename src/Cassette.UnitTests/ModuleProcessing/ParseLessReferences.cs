using Cassette.Utilities;
using Moq;
using Xunit;

namespace Cassette.ModuleProcessing
{
    public class ParseLessReferences_Tests
    {
        [Fact]
        public void ProcessAddsReferencesToLessAssetInModule()
        {
            var asset = new Mock<IAsset>();
            asset.SetupGet(a => a.SourceFilename).Returns("asset.less");

            var lessSource = @"
// @reference ""another1.less"";
// @reference '/another2.less';
// @reference '../test/another3.less';

#foo {}
// References are only allowed at the top of the file. Ignore others...
// @reference ""ignored.less"";
";
            asset.Setup(a => a.OpenStream())
                 .Returns(lessSource.AsStream());
            var module = new Module("", Mock.Of<IFileSystem>());
            module.Assets.Add(asset.Object);

            var processor = new ParseLessReferences();
            processor.Process(module, Mock.Of<ICassetteApplication>());

            asset.Verify(a => a.AddReference("another1.less", 2));
            asset.Verify(a => a.AddReference("/another2.less", 3));
            asset.Verify(a => a.AddReference("../test/another3.less", 4));
            asset.Verify(a => a.AddReference("ignored.less", It.IsAny<int>()), Times.Never());
        }
    }
}
