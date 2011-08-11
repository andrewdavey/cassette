using System;
using System.Linq;
using System.Xml.Linq;
using Moq;
using Should;
using Xunit;

namespace Cassette.Persistence
{
    public class CreateManifestVisitor_Tests
    {
        Module module = new Module("", Mock.Of<IFileSystem>());

        [Fact]
        public void GivenModuleAndAssets_CreateManifestVisitorCreatesXElement()
        {
            var visitor = new CreateManifestVisitor();
            var asset = StubAsset("asset.js");
            module.Assets.Add(asset.Object);
            
            var element = visitor.CreateManifest(module);

            element.ToString(SaveOptions.DisableFormatting)
                   .ShouldEqual("<module directory=\"\"><asset filename=\"asset.js\" /></module>");
        }

        [Fact]
        public void ReferencesToModulesAreSerializedOnce()
        {
            var visitor = new CreateManifestVisitor();
            var asset1 = StubAsset("asset1.js", a => new AssetReference("module-b", a, 1, AssetReferenceType.DifferentModule));
            var asset2 = StubAsset("asset2.js", a => new AssetReference("module-b", a, 1, AssetReferenceType.DifferentModule));
            module.Assets.Add(asset1.Object);
            module.Assets.Add(asset2.Object);

            var element = visitor.CreateManifest(module);

            element.ToString(SaveOptions.DisableFormatting)
                   .ShouldEqual("<module directory=\"\"><reference path=\"module-b\" /><asset filename=\"asset1.js\" /><asset filename=\"asset2.js\" /></module>");
        }

        Mock<IAsset> StubAsset(string filename, params Func<IAsset, AssetReference>[] createReferences)
        {
            var asset = new Mock<IAsset>();
            asset.SetupGet(a => a.SourceFilename).Returns(filename);
            asset.Setup(a => a.Accept(It.IsAny<IAssetVisitor>()))
                 .Callback<IAssetVisitor>(v => v.Visit(asset.Object));
            var references = createReferences.Select(c => c(asset.Object));
            asset.SetupGet(a => a.References).Returns(references);
            return asset;
        }
    }
}
