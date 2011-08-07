using System.Xml.Linq;
using Moq;
using Should;
using Xunit;

namespace Cassette.Persistence
{
    public class CreateManifestVisitor_Tests
    {
        [Fact]
        public void GivenModuleAndAssets_CreateManifestVisitorCreatesXElement()
        {
            var visitor = new CreateManifestVisitor();
            var module = new Module("", _ => null);
            var asset = new Mock<IAsset>();
            asset.Setup(a => a.Accept(It.IsAny<IAssetVisitor>()))
                 .Callback<IAssetVisitor>(v => v.Visit(asset.Object));
            asset.SetupGet(a => a.SourceFilename).Returns("asset.js");
            module.Assets.Add(asset.Object);
            
            var element = visitor.CreateManifest(module);

            element.ToString(SaveOptions.DisableFormatting)
                   .ShouldEqual("<module directory=\"\"><asset filename=\"asset.js\" /></module>");
        }
    }
}
