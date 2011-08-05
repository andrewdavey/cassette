using System.IO;
using Moq;
using Xunit;

namespace Cassette
{
    public class ParseJavaScriptReferences_Tests
    {
        [Fact]
        public void ProcessAddsReferencesToJavaScriptAssetInModule()
        {
            var asset = new Mock<IAsset>();
            asset.SetupGet(a => a.SourceFilename).Returns("c:\\asset.js");

            var javaScriptSource = @"
/// <reference path=""another1.js""/>
///   <reference path=""/another2.js"">
/// <reference path='../test/another3.js'/>

function dummy() {}
// References are only allowed at the top of the file. Ignore others...
/// <reference path=""ignored.js""/>
";
            asset.Setup(a => a.OpenStream())
                 .Returns(CreateStream(javaScriptSource));
            var module = new Module("c:\\");
            module.Assets.Add(asset.Object);

            var processor = new ParseJavaScriptReferences();
            processor.Process(module);

            asset.Verify(a => a.AddReference("another1.js"));
            asset.Verify(a => a.AddReference("/another2.js"));
            asset.Verify(a => a.AddReference("../test/another3.js"));
            asset.Verify(a => a.AddReference("ignored.js"), Times.Never());
        }

        Stream CreateStream(string text)
        {
            var source = new MemoryStream();
            var writer = new StreamWriter(source);
            writer.Write(text);
            writer.Flush();
            source.Position = 0;
            return source;
        }
    }
}
