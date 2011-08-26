using System;
using Moq;
using Should;
using Xunit;

namespace Cassette
{
    public class Module_Tests
    {
        [Fact]
        public void ConstructorNormalizesDirectoryPathByRemovingTrailingBackSlash()
        {
            var module = new Module("test\\");
            module.Path.ShouldEqual("test");
        }

        [Fact]
        public void ConstructorNormalizesDirectoryPathByRemovingTrailingForwardSlash()
        {
            var module = new Module("test/");
            module.Path.ShouldEqual("test");
        }

        [Fact]
        public void ContainsPathOfAssetInModule_ReturnsTrue()
        {
            var module = new Module("test");
            var asset = new Mock<IAsset>();
            asset.Setup(a => a.Accept(It.IsAny<IAssetVisitor>()))
                 .Callback<IAssetVisitor>(v => v.Visit(asset.Object));
            asset.Setup(a => a.SourceFilename).Returns("asset.js");
            module.Assets.Add(asset.Object);

            module.ContainsPath("~\\test\\asset.js").ShouldBeTrue();
        }

        [Fact]
        public void ContainsPathOfAssetInModuleWithForwardSlash_ReturnsTrue()
        {
            var module = new Module("test");
            var asset = new Mock<IAsset>();
            asset.Setup(a => a.Accept(It.IsAny<IAssetVisitor>()))
                 .Callback<IAssetVisitor>(v => v.Visit(asset.Object));
            asset.Setup(a => a.SourceFilename).Returns("asset.js");
            module.Assets.Add(asset.Object);

            module.ContainsPath("~/test/asset.js").ShouldBeTrue();
        }

        [Fact]
        public void ContainsPathOfAssetInModuleWithDifferentCasing_ReturnsTrue()
        {
            var module = new Module("test");
            var asset = new Mock<IAsset>();
            asset.Setup(a => a.Accept(It.IsAny<IAssetVisitor>()))
                 .Callback<IAssetVisitor>(v => v.Visit(asset.Object));
            asset.Setup(a => a.SourceFilename).Returns("asset.js");
            module.Assets.Add(asset.Object);

            module.ContainsPath("~\\TEST\\ASSET.js").ShouldBeTrue();
        }

        [Fact]
        public void ContainsPathOfAssetNotInModule_ReturnsFalse()
        {
            var module = new Module("test");

            module.ContainsPath("~\\test\\not-in-module.js").ShouldBeFalse();
        }

        [Fact]
        public void ContainsPathOfJustTheModuleItself_ReturnsTrue()
        {
            var module = new Module("test");

            module.ContainsPath("~\\test").ShouldBeTrue();
        }

        [Fact]
        public void ContainsPathOfJustTheModuleItselfWithDifferentCasing_ReturnsTrue()
        {
            var module = new Module("test");

            module.ContainsPath("~\\TEST").ShouldBeTrue();
        }

        [Fact]
        public void ContainsPathOfJustTheModuleItselfWithTrailingSlash_ReturnsTrue()
        {
            var module = new Module("test");

            module.ContainsPath("~\\test\\").ShouldBeTrue();
        }

        [Fact]
        public void FindAssetByPathReturnsAssetWithMatchingFilename()
        {
            var module = new Module("test");
            var asset = new Mock<IAsset>();
            asset.Setup(a => a.SourceFilename).Returns("asset.js");
            module.Assets.Add(asset.Object);

            module.FindAssetByPath("asset.js").ShouldBeSameAs(asset.Object);
        }

        [Fact]
        public void WhenFindAssetByPathNotFound_ThenNullReturned()
        {
            var module = new Module("test");

            module.FindAssetByPath("notfound.js").ShouldBeNull();
        }

        [Fact]
        public void GivenAssetInSubDirectory_WhenFindAssetByPath_ThenAssetWithMatchingFilenameIsReturned()
        {
            var module = new Module("test");
            var asset = new Mock<IAsset>();
            asset.Setup(a => a.SourceFilename).Returns("sub\\asset.js");
            module.Assets.Add(asset.Object);

            module.FindAssetByPath("sub\\asset.js").ShouldBeSameAs(asset.Object);
        }

        [Fact]
        public void GivenAssetInSubDirectory_WhenFindAssetByPathWithForwardSlashes_ThenAssetWithMatchingFilenameIsReturned()
        {
            var module = new Module("test");
            var asset = new Mock<IAsset>();
            asset.Setup(a => a.SourceFilename).Returns("sub\\asset.js");
            module.Assets.Add(asset.Object);

            module.FindAssetByPath("sub/asset.js").ShouldBeSameAs(asset.Object);
        }

        [Fact]
        public void AcceptCallsVisitOnVistor()
        {
            var visitor = new Mock<IAssetVisitor>();
            var module = new Module("test");

            module.Accept(visitor.Object);

            visitor.Verify(v => v.Visit(module));
        }

        [Fact]
        public void AcceptCallsAcceptForEachAsset()
        {
            var visitor = new Mock<IAssetVisitor>();
            var module = new Module("test");
            var asset1 = new Mock<IAsset>();
            var asset2 = new Mock<IAsset>();
            module.Assets.Add(asset1.Object);
            module.Assets.Add(asset2.Object);
            
            module.Accept(visitor.Object);

            asset1.Verify(a => a.Accept(visitor.Object));
            asset2.Verify(a => a.Accept(visitor.Object));
        }

        [Fact]
        public void DisposeDisposesAllDisposableAssets()
        {
            var module = new Module("");
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
