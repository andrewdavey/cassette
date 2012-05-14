﻿using System;
using System.IO;
using System.Linq;
using Cassette.Utilities;
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
            var bundle = new TestableBundle("~\\test\\");
            bundle.Path.ShouldEqual("~/test");
        }

        [Fact]
        public void ConstructorNormalizesDirectoryPathByRemovingTrailingForwardSlash()
        {
            var bundle = new TestableBundle("~/test/");
            bundle.Path.ShouldEqual("~/test");
        }

        [Fact]
        public void ConstructorNormalizesToForwardSlashes()
        {
            var bundle = new TestableBundle("~/test/foo\\bar");
            bundle.Path.ShouldEqual("~/test/foo/bar");
        }

        [Fact]
        public void ConstructorDoesNotNormalizeUrls()
        {
            var bundle = new TestableBundle("http://test.com/api.js");
            bundle.Path.ShouldEqual("http://test.com/api.js");
        }

        [Fact]
        public void BundlePathIsConvertedToBeApplicationRelative()
        {
            var bundle = new TestableBundle("test");
            bundle.Path.ShouldEqual("~/test");
        }

        [Fact]
        public void ContainsPathOfAssetInBundle_ReturnsTrue()
        {
            var bundle = new TestableBundle("~/test");
            var asset = new Mock<IAsset>();
            asset.Setup(a => a.Accept(It.IsAny<IBundleVisitor>()))
                 .Callback<IBundleVisitor>(v => v.Visit(asset.Object));
            asset.Setup(a => a.Path).Returns("~/test/asset.js");
            bundle.Assets.Add(asset.Object);

            bundle.ContainsPath("~\\test\\asset.js").ShouldBeTrue();
        }

        [Fact]
        public void ContainsPathOfAssetInBundleWithForwardSlash_ReturnsTrue()
        {
            var bundle = new TestableBundle("~/test");
            var asset = new Mock<IAsset>();
            asset.Setup(a => a.Accept(It.IsAny<IBundleVisitor>()))
                 .Callback<IBundleVisitor>(v => v.Visit(asset.Object));
            asset.Setup(a => a.Path).Returns("~/test/asset.js");
            bundle.Assets.Add(asset.Object);

            bundle.ContainsPath("~/test/asset.js").ShouldBeTrue();
        }

        [Fact]
        public void ContainsPathOfAssetInBundleWithDifferentCasing_ReturnsTrue()
        {
            var bundle = new TestableBundle("~/test");
            var asset = new Mock<IAsset>();
            asset.Setup(a => a.Accept(It.IsAny<IBundleVisitor>()))
                 .Callback<IBundleVisitor>(v => v.Visit(asset.Object));
            asset.Setup(a => a.Path).Returns("~/test/asset.js");
            bundle.Assets.Add(asset.Object);

            bundle.ContainsPath("~\\TEST\\ASSET.js").ShouldBeTrue();
        }

        [Fact]
        public void ContainsPathOfAssetNotInBundle_ReturnsFalse()
        {
            var bundle = new TestableBundle("~/test");

            bundle.ContainsPath("~\\test\\not-in-bundle.js").ShouldBeFalse();
        }

        [Fact]
        public void ContainsPathOfJustTheBundleItself_ReturnsTrue()
        {
            var bundle = new TestableBundle("~/test");

            bundle.ContainsPath("~/test").ShouldBeTrue();
        }

        [Fact]
        public void ContainsPathOfJustTheBundleItselfWithBackSlashes_ReturnsTrue()
        {
            var bundle = new TestableBundle("~/test");

            bundle.ContainsPath("~\\test").ShouldBeTrue();
        }

        [Fact]
        public void ContainsPathOfJustTheBundleItselfWithDifferentCasing_ReturnsTrue()
        {
            var bundle = new TestableBundle("~/test");

            bundle.ContainsPath("~\\TEST").ShouldBeTrue();
        }

        [Fact]
        public void ContainsPathOfJustTheBundleItselfWithTrailingSlash_ReturnsTrue()
        {
            var bundle = new TestableBundle("~/test");

            bundle.ContainsPath("~\\test\\").ShouldBeTrue();
        }

        [Fact]
        public void ContainsRelativePathToAsset_ReturnsTrue()
        {
            var bundle = new TestableBundle("~/test");
            var asset = new StubAsset("~/test/asset.js");
            bundle.Assets.Add(asset);

            bundle.ContainsPath("asset.js").ShouldBeTrue();
        }

        [Fact]
        public void FindAssetByPathReturnsAssetWithMatchingFilename()
        {
            var bundle = new TestableBundle("~/test");
            var asset = new StubAsset("~/test/asset.js");
            bundle.Assets.Add(asset);

            bundle.FindAssetByPath("~/test/asset.js").ShouldBeSameAs(asset);
        }

        [Fact]
        public void WhenFindAssetByPathNotFound_ThenNullReturned()
        {
            var bundle = new TestableBundle("~/test");

            bundle.FindAssetByPath("~/test/notfound.js").ShouldBeNull();
        }

        [Fact]
        public void GivenAssetInSubDirectory_WhenFindAssetByPathWithBackSlashes_ThenAssetWithMatchingFilenameIsReturned()
        {
            var bundle = new TestableBundle("~/test");
            var asset = new StubAsset("~/test/sub/asset.js");
            bundle.Assets.Add(asset);

            bundle.FindAssetByPath("~\\test\\sub\\asset.js").ShouldBeSameAs(asset);
        }

        [Fact]
        public void GivenAssetInSubDirectory_WhenFindAssetByPath_ThenAssetWithMatchingFilenameIsReturned()
        {
            var bundle = new TestableBundle("~/test");
            var asset = new StubAsset("~/test/sub/asset.js");
            bundle.Assets.Add(asset);

            bundle.FindAssetByPath("~/test/sub/asset.js").ShouldBeSameAs(asset);
        }

        [Fact]
        public void GivenConcatenatedAsset_WhenFindAssetByPath_ThenSourceAssetsAreSearched()
        {
            var bundle = new TestableBundle("~/test");
            var asset1 = new StubAsset("~/test/asset1.js");
            var asset2 = new StubAsset("~/test/asset2.js");

            // Simulate concatenated asset. We only need the Accept method to visit each child.
            var concatenatedAsset = new Mock<IAsset>();
            concatenatedAsset.Setup(a => a.Accept(It.IsAny<IBundleVisitor>()))
                .Callback<IBundleVisitor>(v =>
                {
                    v.Visit(asset1);
                    v.Visit(asset2);
                });
            bundle.Assets.Add(concatenatedAsset.Object);

            bundle.FindAssetByPath("~/test/asset2.js").ShouldBeSameAs(asset2);
        }

        [Fact]
        public void AcceptCallsVisitOnVistor()
        {
            var visitor = new Mock<IBundleVisitor>();
            var bundle = new TestableBundle("~/test");

            bundle.Accept(visitor.Object);

            visitor.Verify(v => v.Visit(bundle));
        }

        [Fact]
        public void AcceptCallsAcceptForEachAsset()
        {
            var visitor = new Mock<IBundleVisitor>();
            var bundle = new TestableBundle("~/test");
            var asset1 = new Mock<IAsset>();
            var asset2 = new Mock<IAsset>();
            bundle.Assets.Add(asset1.Object);
            bundle.Assets.Add(asset2.Object);
            
            bundle.Accept(visitor.Object);

            asset1.Verify(a => a.Accept(visitor.Object));
            asset2.Verify(a => a.Accept(visitor.Object));
        }

        [Fact]
        public void OpenStreamReturnsSingleAssetOpenStreamResult()
        {
            var bundle = new TestableBundle("~");
            var asset = new Mock<IAsset>();
            using (var stream = new MemoryStream())
            {
                asset.Setup(a => a.OpenStream()).Returns(stream);
                bundle.Assets.Add(asset.Object);
                bundle.Process(new CassetteSettings());

                var actualStream = bundle.OpenStream();

                actualStream.ShouldBeSameAs(stream);
            }
        }

        [Fact]
        public void DisposeDisposesAllDisposableAssets()
        {
            var bundle = new TestableBundle("~");
            var asset1 = new Mock<IDisposable>();
            var asset2 = new Mock<IDisposable>();
            var asset3 = new Mock<IAsset>(); // Not disposable; Tests for incorrect casting to IDisposable.
            bundle.Assets.Add(asset1.As<IAsset>().Object);
            bundle.Assets.Add(asset2.As<IAsset>().Object);
            bundle.Assets.Add(asset3.Object);

            ((IDisposable)bundle).Dispose();

            asset1.Verify(a => a.Dispose());
            asset2.Verify(a => a.Dispose());
        }

        [Fact]
        public void WhenProcess_ThenIsProcessedIsTrue()
        {
            var bundle = new TestableBundle("~");
            bundle.Process(new CassetteSettings());
            bundle.IsProcessed.ShouldBeTrue();
        }

        [Fact]
        public void GivenBundleIsProcessed_WhenProcess_ThenThrowInvalidOperationException()
        {
            var bundle = new TestableBundle("~");
            bundle.Process(new CassetteSettings());
           
            Assert.Throws<InvalidOperationException>(
                () => bundle.Process(new CassetteSettings())
            );
        }

        [Fact]
        public void GivenNewBundle_ThenEmptyHtmlAttributes()
        {
            var bundle = new TestableBundle("~/test");

            bundle.HtmlAttributes.ShouldBeEmpty();
        }

        [Fact]
        public void UrlIsBundleTypeAndBase64HashAndPath()
        {
            var bundle = new TestableBundle("~/path")
            {
                Hash = new byte[] { 1, 2, 3 }
            };
            var hash = bundle.Hash.ToUrlSafeBase64String();
            bundle.Url.ShouldEqual("testable/" + hash + "/path");
        }
    }

    public class Bundle_AddReference_Tests
    {
        [Fact]
        public void StoresReferences()
        {
            var bundle = new TestableBundle("~/bundle");
            bundle.AddReference("~\\test");
            bundle.AddReference("~\\other");
            bundle.References.SequenceEqual(new[] { "~/test", "~/other" }).ShouldBeTrue();
        }

        [Fact]
        public void ReferenceStartingWithSlashIsConvertedToAppRelative()
        {
            var bundle = new TestableBundle("~/bundle");
            bundle.AddReference("/test");
            bundle.References.Single().ShouldEqual("~/test");
        }

        [Fact]
        public void BundleRelativePathIsConvertedToAppRelative()
        {
            var bundle = new TestableBundle("~/bundle");
            bundle.AddReference("../lib");
            bundle.References.Single().ShouldEqual("~/lib");
        }

        [Fact]
        public void TrailingSlashIsRemoved()
        {
            var bundle = new TestableBundle("~/bundle");
            bundle.AddReference("../lib/");
            bundle.References.Single().ShouldEqual("~/lib");
        }

        [Fact]
        public void UrlIsNotConverted()
        {
            var bundle = new TestableBundle("~/bundle");
            bundle.AddReference("http://test.com/");
            bundle.References.Single().ShouldEqual("http://test.com/");
        }
    }
}