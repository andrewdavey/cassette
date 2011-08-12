using System.Xml.Linq;
using Moq;
using Should;
using Xunit;

namespace Cassette.Persistence
{
    public class CreateManifestVisitor_Tests
    {
        Module module = new Module("test", Mock.Of<IFileSystem>());

        [Fact]
        public void GivenModuleAndAssets_CreateManifestVisitorCreatesXElement()
        {
            var visitor = new CreateManifestVisitor(m => new string[0]);
            var asset = StubAsset("asset.js");
            module.Assets.Add(asset.Object);
            
            var element = visitor.CreateManifest(module);

            element.ToString(SaveOptions.DisableFormatting)
                   .ShouldEqual("<module directory=\"test\" hash=\"\"><asset filename=\"asset.js\" /></module>");
        }

        [Fact]
        public void ModuleReferencesAreSerialized()
        {
            var visitor = new CreateManifestVisitor(m => new[] { "module-b" });
            var asset = StubAsset("asset.js");
            module.Assets.Add(asset.Object);

            var element = visitor.CreateManifest(module);

            element.ToString(SaveOptions.DisableFormatting)
                   .ShouldEqual("<module directory=\"test\" hash=\"\"><reference path=\"module-b\" /><asset filename=\"asset.js\" /></module>");
        }

        Mock<IAsset> StubAsset(string filename)
        {
            var asset = new Mock<IAsset>();
            asset.SetupGet(a => a.SourceFilename).Returns(filename);
            asset.Setup(a => a.Accept(It.IsAny<IAssetVisitor>()))
                 .Callback<IAssetVisitor>(v => v.Visit(asset.Object));
            return asset;
        }
    }
}
