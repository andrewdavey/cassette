using Cassette.Utilities;
using Moq;
using Xunit;

namespace Cassette.ModuleProcessing
{
    public class ParseJavaScriptReferences_Tests
    {
        [Fact]
        public void ProcessAddsReferencesToJavaScriptAssetInModule()
        {
            var asset = new Mock<IAsset>();
            asset.SetupGet(a => a.SourceFilename).Returns("asset.js");

            var javaScriptSource = @"
/// <reference path=""another1.js""/>
///   <reference path=""/another2.js"">
/// <reference path='../test/another3.js'/>

function dummy() {}
// References are only allowed at the top of the file. Ignore others...
/// <reference path=""ignored.js""/>
";
            asset.Setup(a => a.OpenStream())
                 .Returns(javaScriptSource.AsStream());
            var module = new Module("", Mock.Of<IFileSystem>());
            module.Assets.Add(asset.Object);

            var processor = new ParseJavaScriptReferences();
            processor.Process(module);

            asset.Verify(a => a.AddReference("another1.js", 2));
            asset.Verify(a => a.AddReference("/another2.js", 3));
            asset.Verify(a => a.AddReference("../test/another3.js", 4));
            asset.Verify(a => a.AddReference("ignored.js", It.IsAny<int>()), Times.Never());
        }
    }
}
