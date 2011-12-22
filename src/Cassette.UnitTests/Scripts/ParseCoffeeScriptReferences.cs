using Cassette.Configuration;
using Cassette.Utilities;
using Moq;
using Xunit;

namespace Cassette.Scripts
{
    public class ParseCoffeeScriptReferences_Tests
    {
        [Fact]
        public void ProcessAddsReferencesToCoffeeScriptAssetInBundle()
        {
            var asset = new Mock<IAsset>();
            asset.SetupGet(a => a.SourceFile.FullPath).Returns("~/asset.coffee");

            var coffeeScriptSource = @"
# @reference ""another1.js""
# @reference 'another2.coffee'
# @reference another3.coffee

class Foo
";
            asset.Setup(a => a.OpenStream())
                 .Returns(coffeeScriptSource.AsStream());
            var bundle = new ScriptBundle("~");
            bundle.Assets.Add(asset.Object);

            var processor = new ParseCoffeeScriptReferences();
            processor.Process(bundle, new CassetteSettings(""));

            asset.Verify(a => a.AddReference("another1.js", 2));
            asset.Verify(a => a.AddReference("another2.coffee", 3));
            asset.Verify(a => a.AddReference("another3.coffee", 4));
        }
    }
}

