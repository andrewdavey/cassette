#region License
/*
Copyright 2011 Andrew Davey

This file is part of Cassette.

Cassette is free software: you can redistribute it and/or modify it under the 
terms of the GNU General Public License as published by the Free Software 
Foundation, either version 3 of the License, or (at your option) any later 
version.

Cassette is distributed in the hope that it will be useful, but WITHOUT ANY 
WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS 
FOR A PARTICULAR PURPOSE. See the GNU General Public License for more details.

You should have received a copy of the GNU General Public License along with 
Cassette. If not, see http://www.gnu.org/licenses/.
*/
#endregion

using System;
using System.Linq;
using Moq;
using Should;
using Xunit;

namespace Cassette
{
    public class Bundle_Tests
    {
        [Fact]
        public void ConstructorNormalizesDirectoryPathByRemovingTrailingBackSlash()
        {
            var bundle = new Bundle("~\\test\\");
            bundle.Path.ShouldEqual("~/test");
        }

        [Fact]
        public void ConstructorNormalizesDirectoryPathByRemovingTrailingForwardSlash()
        {
            var bundle = new Bundle("~/test/");
            bundle.Path.ShouldEqual("~/test");
        }

        [Fact]
        public void ConstructorNormalizesToForwardSlashes()
        {
            var bundle = new Bundle("~/test/foo\\bar");
            bundle.Path.ShouldEqual("~/test/foo/bar");
        }

        [Fact]
        public void ConstructorDoesNotNormalizeUrls()
        {
            var bundle = new Bundle("http://test.com/api.js");
            bundle.Path.ShouldEqual("http://test.com/api.js");
        }

        [Fact]
        public void BundlePathIsConvertedToBeApplicationRelative()
        {
            var bundle = new Bundle("test");
            bundle.Path.ShouldEqual("~/test");
        }

        [Fact]
        public void CanCreateWithAssetSources()
        {
            var initializer = Mock.Of<IBundleInitializer>();
            var bundle = new Bundle("~/test", new[] { initializer });

            bundle.BundleInitializers.SequenceEqual(new[] { initializer }).ShouldBeTrue();
        }

        [Fact]
        public void ContainsPathOfAssetInBundle_ReturnsTrue()
        {
            var bundle = new Bundle("~/test");
            var asset = new Mock<IAsset>();
            asset.Setup(a => a.Accept(It.IsAny<IAssetVisitor>()))
                 .Callback<IAssetVisitor>(v => v.Visit(asset.Object));
            asset.Setup(a => a.SourceFilename).Returns("~/test/asset.js");
            bundle.Assets.Add(asset.Object);

            bundle.ContainsPath("~\\test\\asset.js").ShouldBeTrue();
        }

        [Fact]
        public void ContainsPathOfAssetInBundleWithForwardSlash_ReturnsTrue()
        {
            var bundle = new Bundle("~/test");
            var asset = new Mock<IAsset>();
            asset.Setup(a => a.Accept(It.IsAny<IAssetVisitor>()))
                 .Callback<IAssetVisitor>(v => v.Visit(asset.Object));
            asset.Setup(a => a.SourceFilename).Returns("~/test/asset.js");
            bundle.Assets.Add(asset.Object);

            bundle.ContainsPath("~/test/asset.js").ShouldBeTrue();
        }

        [Fact]
        public void ContainsPathOfAssetInBundleWithDifferentCasing_ReturnsTrue()
        {
            var bundle = new Bundle("~/test");
            var asset = new Mock<IAsset>();
            asset.Setup(a => a.Accept(It.IsAny<IAssetVisitor>()))
                 .Callback<IAssetVisitor>(v => v.Visit(asset.Object));
            asset.Setup(a => a.SourceFilename).Returns("~/test/asset.js");
            bundle.Assets.Add(asset.Object);

            bundle.ContainsPath("~\\TEST\\ASSET.js").ShouldBeTrue();
        }

        [Fact]
        public void ContainsPathOfAssetNotInBundle_ReturnsFalse()
        {
            var bundle = new Bundle("~/test");

            bundle.ContainsPath("~\\test\\not-in-bundle.js").ShouldBeFalse();
        }

        [Fact]
        public void ContainsPathOfJustTheBundleItself_ReturnsTrue()
        {
            var bundle = new Bundle("~/test");

            bundle.ContainsPath("~/test").ShouldBeTrue();
        }

        [Fact]
        public void ContainsPathOfJustTheBundleItselfWithBackSlashes_ReturnsTrue()
        {
            var bundle = new Bundle("~/test");

            bundle.ContainsPath("~\\test").ShouldBeTrue();
        }

        [Fact]
        public void ContainsPathOfJustTheBundleItselfWithDifferentCasing_ReturnsTrue()
        {
            var bundle = new Bundle("~/test");

            bundle.ContainsPath("~\\TEST").ShouldBeTrue();
        }

        [Fact]
        public void ContainsPathOfJustTheBundleItselfWithTrailingSlash_ReturnsTrue()
        {
            var bundle = new Bundle("~/test");

            bundle.ContainsPath("~\\test\\").ShouldBeTrue();
        }

        [Fact]
        public void FindAssetByPathReturnsAssetWithMatchingFilename()
        {
            var bundle = new Bundle("~/test");
            var asset = new Mock<IAsset>();
            asset.Setup(a => a.SourceFilename).Returns("~/test/asset.js");
            bundle.Assets.Add(asset.Object);

            bundle.FindAssetByPath("~/test/asset.js").ShouldBeSameAs(asset.Object);
        }

        [Fact]
        public void WhenFindAssetByPathNotFound_ThenNullReturned()
        {
            var bundle = new Bundle("~/test");

            bundle.FindAssetByPath("~/test/notfound.js").ShouldBeNull();
        }

        [Fact]
        public void GivenAssetInSubDirectory_WhenFindAssetByPathWithBackSlashes_ThenAssetWithMatchingFilenameIsReturned()
        {
            var bundle = new Bundle("~/test");
            var asset = new Mock<IAsset>();
            asset.Setup(a => a.SourceFilename).Returns("~/test/sub/asset.js");
            bundle.Assets.Add(asset.Object);

            bundle.FindAssetByPath("~\\test\\sub\\asset.js").ShouldBeSameAs(asset.Object);
        }

        [Fact]
        public void GivenAssetInSubDirectory_WhenFindAssetByPath_ThenAssetWithMatchingFilenameIsReturned()
        {
            var bundle = new Bundle("~/test");
            var asset = new Mock<IAsset>();
            asset.Setup(a => a.SourceFilename).Returns("~/test/sub/asset.js");
            bundle.Assets.Add(asset.Object);

            bundle.FindAssetByPath("~/test/sub/asset.js").ShouldBeSameAs(asset.Object);
        }

        [Fact]
        public void AcceptCallsVisitOnVistor()
        {
            var visitor = new Mock<IAssetVisitor>();
            var bundle = new Bundle("~/test");

            bundle.Accept(visitor.Object);

            visitor.Verify(v => v.Visit(bundle));
        }

        [Fact]
        public void AcceptCallsAcceptForEachAsset()
        {
            var visitor = new Mock<IAssetVisitor>();
            var bundle = new Bundle("~/test");
            var asset1 = new Mock<IAsset>();
            var asset2 = new Mock<IAsset>();
            bundle.Assets.Add(asset1.Object);
            bundle.Assets.Add(asset2.Object);
            
            bundle.Accept(visitor.Object);

            asset1.Verify(a => a.Accept(visitor.Object));
            asset2.Verify(a => a.Accept(visitor.Object));
        }

        [Fact]
        public void HashIsHashOfFirstAsset()
        {
            var bundle = new Bundle("~/test");
            var asset = new Mock<IAsset>();
            asset.SetupGet(a => a.Hash).Returns(new byte[] {1, 2, 3});
            bundle.Assets.Add(asset.Object);
            bundle.Hash.SequenceEqual(new byte[] {1, 2, 3}).ShouldBeTrue();
        }

        [Fact]
        public void HashThrowsInvalidOperationExceptionWhenMoreThanOneAsset()
        {
            var bundle = new Bundle("~/test");
            bundle.Assets.Add(Mock.Of<IAsset>());
            bundle.Assets.Add(Mock.Of<IAsset>());
            Assert.Throws<InvalidOperationException>(delegate
            {
                var temp = bundle.Hash;
            });
        }

        [Fact]
        public void DisposeDisposesAllDisposableAssets()
        {
            var bundle = new Bundle("~");
            var asset1 = new Mock<IDisposable>();
            var asset2 = new Mock<IDisposable>();
            var asset3 = new Mock<IAsset>(); // Not disposable; Tests for incorrect casting to IDisposable.
            bundle.Assets.Add(asset1.As<IAsset>().Object);
            bundle.Assets.Add(asset2.As<IAsset>().Object);
            bundle.Assets.Add(asset3.Object);

            bundle.Dispose();

            asset1.Verify(a => a.Dispose());
            asset2.Verify(a => a.Dispose());
        }
    }

    public class Bundle_AddReferences_Tests
    {
        [Fact]
        public void StoresReferences()
        {
            var bundle = new Bundle("~/bundle");
            bundle.AddReferences(new[] { "~\\test", "~\\other" });
            bundle.References.SequenceEqual(new[] { "~/test", "~/other" }).ShouldBeTrue();
        }

        [Fact]
        public void ReferenceStartingWithSlashIsConvertedToAppRelative()
        {
            var bundle = new Bundle("~/bundle");
            bundle.AddReferences(new[] { "/test" });
            bundle.References.Single().ShouldEqual("~/test");
        }

        [Fact]
        public void BundleRelativePathIsConvertedToAppRelative()
        {
            var bundle = new Bundle("~/bundle");
            bundle.AddReferences(new[] { "../lib" });
            bundle.References.Single().ShouldEqual("~/lib");
        }

        [Fact]
        public void TrailingSlashIsRemoved()
        {
            var bundle = new Bundle("~/bundle");
            bundle.AddReferences(new[] { "../lib/" });
            bundle.References.Single().ShouldEqual("~/lib");
        }

        [Fact]
        public void UrlIsNotConverted()
        {
            var bundle = new Bundle("~/bundle");
            bundle.AddReferences(new[] { "http://test.com/" });
            bundle.References.Single().ShouldEqual("http://test.com/");
        }
    }

    public class Bundle_AssetSources_Tests
    {
        [Fact]
        public void AssetSourcesIsInitiallyEmpty()
        {
            var bundle = new Bundle("~");
            bundle.BundleInitializers.ShouldBeEmpty();
        }

        [Fact]
        public void GivenAssetSourceAdded_WhenAddAssetsFromSources_ThenInitializeBundleCalled()
        {
            var bundle = new Bundle("~");
            var application = Mock.Of<ICassetteApplication>();
            var initializer = new Mock<IBundleInitializer>();
            bundle.BundleInitializers.Add(initializer.Object);
            
            bundle.Initialize(application);

            initializer.Verify(s => s.InitializeBundle(bundle, application));
        }
    }
}