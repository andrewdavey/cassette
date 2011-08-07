using System;
using Moq;
using Should;
using Xunit;

namespace Cassette
{
    public class Module_Tests
    {
        Func<string, string> getFullPath = _ => null;

        [Fact]
        public void ConstructorNormalizesDirectoryPathByRemovingTrailingBackSlash()
        {
            var module = new Module("test\\", getFullPath);
            module.Directory.ShouldEqual("test");
        }

        [Fact]
        public void ConstructorNormalizesDirectoryPathByRemovingTrailingForwardSlash()
        {
            var module = new Module("test/", getFullPath);
            module.Directory.ShouldEqual("test");
        }

        [Fact]
        public void ContainsPathOfAssetInModule_ReturnsTrue()
        {
            var module = new Module("test", getFullPath);
            var asset = new Mock<IAsset>();
            asset.Setup(a => a.IsFrom("test\\asset.js")).Returns(true);
            module.Assets.Add(asset.Object);

            module.ContainsPath("test\\asset.js").ShouldBeTrue();
        }

        [Fact]
        public void ContainsPathOfAssetInModuleWithDifferentCasing_ReturnsTrue()
        {
            var module = new Module("test", getFullPath);
            var asset = new Mock<IAsset>();
            asset.Setup(a => a.IsFrom("TEST\\ASSET.js")).Returns(true);
            module.Assets.Add(asset.Object);

            module.ContainsPath("TEST\\ASSET.js").ShouldBeTrue();
        }

        [Fact]
        public void ContainsPathOfAssetNotInModule_ReturnsFalse()
        {
            var module = new Module("test", getFullPath);

            module.ContainsPath("test\\no-in-module.js").ShouldBeFalse();
        }

        [Fact]
        public void ContainsPathOfJustTheModuleItself_ReturnsTrue()
        {
            var module = new Module("test", getFullPath);

            module.ContainsPath("test").ShouldBeTrue();
        }

        [Fact]
        public void ContainsPathOfJustTheModuleItselfWithDifferentCasing_ReturnsTrue()
        {
            var module = new Module("test", getFullPath);

            module.ContainsPath("TEST").ShouldBeTrue();
        }

        [Fact]
        public void ContainsPathOfJustTheModuleItselfWithTrailingSlash_ReturnsTrue()
        {
            var module = new Module("test", getFullPath);

            module.ContainsPath("test\\").ShouldBeTrue();
        }

        [Fact]
        public void AcceptCallsVisitOnVistor()
        {
            var visitor = new Mock<IAssetVisitor>();
            var module = new Module("test", getFullPath);

            module.Accept(visitor.Object);

            visitor.Verify(v => v.Visit(module));
        }

        [Fact]
        public void AcceptCallsVisitOnVistorForEachAsset()
        {
            var visitor = new Mock<IAssetVisitor>();
            var module = new Module("test", getFullPath);
            var asset1 = new Mock<IAsset>();
            var asset2 = new Mock<IAsset>();
            module.Assets.Add(asset1.Object);
            module.Assets.Add(asset2.Object);
            
            module.Accept(visitor.Object);

            visitor.Verify(v => v.Visit(asset1.Object));
            visitor.Verify(v => v.Visit(asset2.Object));
        }

        [Fact]
        public void DisposeDisposesAllDisposableAssets()
        {
            var module = new Module("", getFullPath);
            var asset1 = new Mock<IDisposable>();
            var asset2 = new Mock<IDisposable>();
            var asset3 = new Mock<IAsset>(); // Not disposable; Tests for incorrect casting to IDisposable.
            module.Assets.Add(asset1.As<IAsset>().Object);
            module.Assets.Add(asset2.As<IAsset>().Object);
            module.Assets.Add(asset3.Object);

            module.Dispose();

            asset1.Verify(a => a.Dispose());
            asset2.Verify(a => a.Dispose());
        }
    }
}
