using System.Xml.Linq;
using Moq;
using Should;
using Xunit;

namespace Cassette.Persistence
{
    public class CreateManifestVisitor_Tests
    {
        readonly Module module = new Module("test");

        [Fact]
        public void GivenModuleAndAssets_CreateManifestVisitorCreatesXElement()
        {
            var visitor = new CreateManifestVisitor(m => new string[0]);
            var asset = StubAsset("asset.js");
            module.Assets.Add(asset.Object);
            
            var element = visitor.CreateManifest(module);

            element.ToString(SaveOptions.DisableFormatting)
                   .ShouldEqual("<module directory=\"test\" hash=\"01\"><asset filename=\"asset.js\" /></module>");
        }

        [Fact]
        public void ModuleReferencesAreSerialized()
        {
            var visitor = new CreateManifestVisitor(m => new[] { "module-b" });
            var asset = StubAsset("asset.js");
            module.Assets.Add(asset.Object);

            var element = visitor.CreateManifest(module);

            element.ToString(SaveOptions.DisableFormatting)
                   .ShouldEqual("<module directory=\"test\" hash=\"01\"><reference path=\"module-b\" /><asset filename=\"asset.js\" /></module>");
        }

        [Fact]
        public void GivenEmptyModule_ThenModuleHashAttributeIsEmpty()
        {
            var visitor = new CreateManifestVisitor(m => new string[0]);

            var element = visitor.CreateManifest(module);

            element.ToString(SaveOptions.DisableFormatting)
                   .ShouldEqual("<module directory=\"test\" hash=\"\" />");
        }

        Mock<IAsset> StubAsset(string filename)
        {
            var asset = new Mock<IAsset>();
            asset.SetupGet(a => a.SourceFilename).Returns(filename);
            asset.SetupGet(a => a.Hash).Returns(new byte[] { 1 });
            asset.Setup(a => a.Accept(It.IsAny<IAssetVisitor>()))
                 .Callback<IAssetVisitor>(v => v.Visit(asset.Object));
            return asset;
        }
    }
}
