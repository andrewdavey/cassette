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
using System.IO;
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
        public void GivenBundleCreateWithUseDefaultInitializerTrue_WhenInitialize_ThenInitializerRegisteredWithApplicationIsUsed()
        {
            var bundle = new TestableBundle("~/test", true);
            var defaultInitializer = new Mock<IBundleInitializer>();
            var application = new Mock<ICassetteApplication>();
            application.Setup(a => a.GetDefaultBundleInitializer(typeof(TestableBundle)))
                       .Returns(defaultInitializer.Object);

            bundle.Initialize(application.Object);

            defaultInitializer.Verify(i => i.InitializeBundle(bundle, application.Object));
        }

        [Fact]
        public void GivenBundleWithUseDefaultInitializerTrue_WhenInitialize_ThenDefaultBundleInitializerIsNotUsed()
        {
            var bundle = new TestableBundle("~/test", true);
            var initializer = new Mock<IBundleInitializer>();
            bundle.BundleInitializers.Add(initializer.Object);

            var defaultInitializer = new Mock<IBundleInitializer>();
            var application = new Mock<ICassetteApplication>();
            application.Setup(a => a.GetDefaultBundleInitializer(typeof(TestableBundle)))
                       .Returns(defaultInitializer.Object);

            bundle.Initialize(application.Object);

            defaultInitializer.Verify(
                i => i.InitializeBundle(It.IsAny<Bundle>(), It.IsAny<ICassetteApplication>()),
                Times.Never()
            );
        }

        [Fact]
        public void ContainsPathOfAssetInBundle_ReturnsTrue()
        {
            var bundle = new TestableBundle("~/test");
            var asset = new Mock<IAsset>();
            asset.Setup(a => a.Accept(It.IsAny<IAssetVisitor>()))
                 .Callback<IAssetVisitor>(v => v.Visit(asset.Object));
            asset.Setup(a => a.SourceFile.FullPath).Returns("~/test/asset.js");
            bundle.Assets.Add(asset.Object);

            bundle.ContainsPath("~\\test\\asset.js").ShouldBeTrue();
        }

        [Fact]
        public void ContainsPathOfAssetInBundleWithForwardSlash_ReturnsTrue()
        {
            var bundle = new TestableBundle("~/test");
            var asset = new Mock<IAsset>();
            asset.Setup(a => a.Accept(It.IsAny<IAssetVisitor>()))
                 .Callback<IAssetVisitor>(v => v.Visit(asset.Object));
            asset.Setup(a => a.SourceFile.FullPath).Returns("~/test/asset.js");
            bundle.Assets.Add(asset.Object);

            bundle.ContainsPath("~/test/asset.js").ShouldBeTrue();
        }

        [Fact]
        public void ContainsPathOfAssetInBundleWithDifferentCasing_ReturnsTrue()
        {
            var bundle = new TestableBundle("~/test");
            var asset = new Mock<IAsset>();
            asset.Setup(a => a.Accept(It.IsAny<IAssetVisitor>()))
                 .Callback<IAssetVisitor>(v => v.Visit(asset.Object));
            asset.Setup(a => a.SourceFile.FullPath).Returns("~/test/asset.js");
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
            var asset = StubAsset("~/test/asset.js");
            bundle.Assets.Add(asset);

            bundle.ContainsPath("asset.js").ShouldBeTrue();
        }

        [Fact]
        public void FindAssetByPathReturnsAssetWithMatchingFilename()
        {
            var bundle = new TestableBundle("~/test");
            var asset = StubAsset("~/test/asset.js");
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
            var asset = StubAsset("~/test/sub/asset.js");
            bundle.Assets.Add(asset);

            bundle.FindAssetByPath("~\\test\\sub\\asset.js").ShouldBeSameAs(asset);
        }

        [Fact]
        public void GivenAssetInSubDirectory_WhenFindAssetByPath_ThenAssetWithMatchingFilenameIsReturned()
        {
            var bundle = new TestableBundle("~/test");
            var asset = StubAsset("~/test/sub/asset.js");
            bundle.Assets.Add(asset);

            bundle.FindAssetByPath("~/test/sub/asset.js").ShouldBeSameAs(asset);
        }

        [Fact]
        public void GivenConcatenatedAsset_WhenFindAssetByPath_ThenSourceAssetsAreSearched()
        {
            var bundle = new TestableBundle("~/test");
            var asset1 = StubAsset("~/test/asset1.js");
            var asset2 = StubAsset("~/test/asset2.js");

            // Simulate concatenated asset. We only need the Accept method to visit each child.
            var concatenatedAsset = new Mock<IAsset>();
            concatenatedAsset.Setup(a => a.Accept(It.IsAny<IAssetVisitor>()))
                .Callback<IAssetVisitor>(v =>
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
            var visitor = new Mock<IAssetVisitor>();
            var bundle = new TestableBundle("~/test");

            bundle.Accept(visitor.Object);

            visitor.Verify(v => v.Visit(bundle));
        }

        [Fact]
        public void AcceptCallsAcceptForEachAsset()
        {
            var visitor = new Mock<IAssetVisitor>();
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
        public void BundleInitializersIsInitiallyEmpty()
        {
            var bundle = new TestableBundle("~");
            bundle.BundleInitializers.ShouldBeEmpty();
        }

        [Fact]
        public void GivenBundleInitializersAdded_WhenInitialize_ThenInitializeBundleCalled()
        {
            var bundle = new TestableBundle("~");
            var application = Mock.Of<ICassetteApplication>();
            var initializer = new Mock<IBundleInitializer>();
            bundle.BundleInitializers.Add(initializer.Object);

            bundle.Initialize(application);

            initializer.Verify(s => s.InitializeBundle(bundle, application));
        }

        [Fact]
        public void GivenBundleCreatedWithOnlyAPath_WhenInitialize_ThenApplicationGetDefaultBundleInitializerIsUsed()
        {
            var bundle = new TestableBundle("~");
            var application = new Mock<ICassetteApplication>();
            var initializer = new Mock<IBundleInitializer>();
            application.Setup(a => a.GetDefaultBundleInitializer(typeof(TestableBundle)))
                       .Returns(initializer.Object);

            bundle.Initialize(application.Object);

            initializer.Verify(i => i.InitializeBundle(bundle, application.Object));
        }

        [Fact]
        public void GivenBundleCreatedWithOnlyAPathAndInitializersAdded_WhenInitialize_ThenApplicationGetDefaultBundleInitializerIsNotUsed()
        {
            var bundle = new TestableBundle("~");
            var application = new Mock<ICassetteApplication>();
            var initializer = new Mock<IBundleInitializer>();

            bundle.BundleInitializers.Add(Mock.Of<IBundleInitializer>());
            bundle.Initialize(application.Object);

            initializer.Verify(i => i.InitializeBundle(bundle, application.Object), Times.Never());
        }

        [Fact]
        public void HashIsHashOfSingleAsset()
        {
            var bundle = new TestableBundle("~");
            var asset = new Mock<IAsset>();
            var hash = new byte[] { 1, 2, 3 };
            asset.SetupGet(a => a.Hash).Returns(hash);
            bundle.Assets.Add(asset.Object);

            bundle.Hash.ShouldEqual(hash);
        }

        [Fact]
        public void HashThrowsExceptionIfMoreThanOneAsset()
        {
            var bundle = new TestableBundle("~");
            bundle.Assets.Add(Mock.Of<IAsset>());
            bundle.Assets.Add(Mock.Of<IAsset>());

            Assert.Throws<InvalidOperationException>(
                () => bundle.Hash            
            );
        }

        [Fact]
        public void HashThrowsExceptionIfNoAssets()
        {
            var bundle = new TestableBundle("~");

            Assert.Throws<InvalidOperationException>(
                () => bundle.Hash
            );
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

        IAsset StubAsset(string filename)
        {
            var asset = new Mock<IAsset>();
            asset.Setup(a => a.Accept(It.IsAny<IAssetVisitor>()))
                 .Callback<IAssetVisitor>(v => v.Visit(asset.Object));
            asset.Setup(a => a.SourceFile.FullPath).Returns(filename);
            return asset.Object;
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