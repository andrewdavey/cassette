using Cassette.Configuration;
using Cassette.Utilities;
using Moq;
using Xunit;

namespace Cassette.Scripts
{
    public class ParseJavaScriptReferences_Tests
    {
        [Fact]
        public void ProcessAddsReferencesToJavaScriptAssetInBundle()
        {
            var asset = new Mock<IAsset>();
            asset.SetupGet(a => a.Path).Returns("~/asset.js");

            var javaScriptSource = @"
/// <reference path=""another1.js""/>
///   <reference path=""/another2.js"">
/// <reference path='../test/another3.js'/>
";
            asset.Setup(a => a.OpenStream())
                 .Returns(javaScriptSource.AsStream());
            var bundle = new ScriptBundle("~");
            bundle.Assets.Add(asset.Object);

            var processor = new ParseJavaScriptReferences();
            processor.Process(bundle, new CassetteSettings());

            asset.Verify(a => a.AddReference("another1.js", 2));
            asset.Verify(a => a.AddReference("/another2.js", 3));
            asset.Verify(a => a.AddReference("../test/another3.js", 4));
        }
    }
}