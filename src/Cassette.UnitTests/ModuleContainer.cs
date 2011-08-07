using System;
using System.Linq;
using Moq;
using Should;
using Xunit;

namespace Cassette
{
    public class ModuleContainer_Tests
    {
        [Fact]
        public void ValidateAndSortModulesOrdersModulesByDependency()
        {
            var module1 = new Module("module-1", _ => null);
            var asset1 = new Mock<IAsset>();
            asset1.SetupGet(a => a.References)
                  .Returns(new[] { new AssetReference("module-2\\b.js", asset1.Object, 1, AssetReferenceType.DifferentModule) });
            module1.Assets.Add(asset1.Object);
            var module2 = new Module("module-2", _ => null);
            var asset2 = new Mock<IAsset>();
            asset2.Setup(a => a.IsFrom("module-2\\b.js")).Returns(true);
            module2.Assets.Add(asset2.Object);

            var container = new ModuleContainer<Module>(new[] { module1, module2 }, DateTime.UtcNow, "c:\\test");
            container.ValidateAndSortModules();

            var modules = container.ToArray();
            modules[0].ShouldBeSameAs(module2);
            modules[1].ShouldBeSameAs(module1);
        }

        [Fact]
        public void GivenAssetWithUnknownDifferentModuleReference_ThenValidateAndSortModulesThrowsAssetReferenceException()
        {
            var module = new Module("module-1", _ => null);
            var asset = new Mock<IAsset>();
            asset.SetupGet(a => a.SourceFilename).Returns("module-1\\a.js");
            asset.SetupGet(a => a.References)
                  .Returns(new[] { new AssetReference("fail\\fail.js", asset.Object, 0, AssetReferenceType.DifferentModule) });
            module.Assets.Add(asset.Object);

            var exception = Assert.Throws<AssetReferenceException>(delegate
            {
                new ModuleContainer<Module>(new[] { module }, DateTime.UtcNow, "c:\\test").ValidateAndSortModules();
            });
            exception.Message.ShouldEqual("Reference error in \"module-1\\a.js\". Cannot find \"fail\\fail.js\".");
        }

        [Fact]
        public void GivenAssetWithUnknownDifferentModuleReferenceHavingLineNumber_ThenValidateAndSortModulesThrowsAssetReferenceException()
        {
            var module = new Module("module-1", _ => null);
            var asset = new Mock<IAsset>();
            asset.SetupGet(a => a.SourceFilename).Returns("module-1\\a.js");
            asset.SetupGet(a => a.References)
                  .Returns(new[] { new AssetReference("fail\\fail.js", asset.Object, 42, AssetReferenceType.DifferentModule) });
            module.Assets.Add(asset.Object);

            var exception = Assert.Throws<AssetReferenceException>(delegate
            {
                new ModuleContainer<Module>(new[] { module }, DateTime.UtcNow, "c:\\test").ValidateAndSortModules();
            });
            exception.Message.ShouldEqual("Reference error in \"module-1\\a.js\", line 42. Cannot find \"fail\\fail.js\".");
        }
    }
}
