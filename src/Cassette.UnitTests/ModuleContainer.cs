using System.Linq;
using Moq;
using Should;
using Xunit;

namespace Cassette
{
    public class ModuleContainer_Tests
    {
        [Fact]
        public void ConstructorOrdersModulesByDependency()
        {
            var module1 = new Module("c:\\test\\module-1");
            var asset1 = new Mock<IAsset>();
            asset1.SetupGet(a => a.SourceFilename).Returns("c:\\test\\module-1\\a.js");
            asset1.SetupGet(a => a.References)
                  .Returns(new[] { new AssetReference("c:\\test\\module-2\\b.js", AssetReferenceType.DifferentModule) });
            module1.Assets.Add(asset1.Object);
            var module2 = new Module("c:\\test\\module-2");
            var asset2 = new Mock<IAsset>();
            asset2.SetupGet(a => a.SourceFilename).Returns("c:\\test\\module-2\\b.js");
            module2.Assets.Add(asset2.Object);

            var container = new ModuleContainer<Module>(new[] { module1, module2 });

            var modules = container.Modules.ToArray();
            modules[0].ShouldBeSameAs(module2);
            modules[1].ShouldBeSameAs(module1);
        }

        [Fact]
        public void GivenAssetWithUnknownDifferentModuleReference_ThenConstructorThrowsAssetReferenceException()
        {
            var module = new Module("c:\\test\\module-1");
            var asset = new Mock<IAsset>();
            asset.SetupGet(a => a.SourceFilename).Returns("c:\\test\\module-1\\a.js");
            asset.SetupGet(a => a.References)
                  .Returns(new[] { new AssetReference("c:\\test\\fail\\fail.js", AssetReferenceType.DifferentModule) });
            module.Assets.Add(asset.Object);

            var exception = Assert.Throws<AssetReferenceException>(delegate
            {
                new ModuleContainer<Module>(new[] { module });
            });
            exception.Message.ShouldEqual("Reference error in \"c:\\test\\module-1\\a.js\". Cannot find \"c:\\test\\fail\\fail.js\".");
        }

        [Fact]
        public void GivenAssetWithUnknownDifferentModuleReferenceHavingLineNumber_ThenConstructorThrowsAssetReferenceException()
        {
            var module = new Module("c:\\test\\module-1");
            var asset = new Mock<IAsset>();
            asset.SetupGet(a => a.SourceFilename).Returns("c:\\test\\module-1\\a.js");
            asset.SetupGet(a => a.References)
                  .Returns(new[] { new AssetReference("c:\\test\\fail\\fail.js", 42, AssetReferenceType.DifferentModule) });
            module.Assets.Add(asset.Object);

            var exception = Assert.Throws<AssetReferenceException>(delegate
            {
                new ModuleContainer<Module>(new[] { module });
            });
            exception.Message.ShouldEqual("Reference error in \"c:\\test\\module-1\\a.js\", line 42. Cannot find \"c:\\test\\fail\\fail.js\".");
        }
    }
}
