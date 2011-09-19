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
    public class Module_Tests
    {
        [Fact]
        public void ConstructorNormalizesDirectoryPathByRemovingTrailingBackSlash()
        {
            var module = new Module("~\\test\\");
            module.Path.ShouldEqual("~/test");
        }

        [Fact]
        public void ConstructorNormalizesDirectoryPathByRemovingTrailingForwardSlash()
        {
            var module = new Module("~/test/");
            module.Path.ShouldEqual("~/test");
        }

        [Fact]
        public void ConstructorNormalizesToForwardSlashes()
        {
            var module = new Module("~/test/foo\\bar");
            module.Path.ShouldEqual("~/test/foo/bar");
        }

        [Fact]
        public void ConstructorDoesNotNormalizeUrls()
        {
            var module = new Module("http://test.com/api.js");
            module.Path.ShouldEqual("http://test.com/api.js");
        }

        [Fact]
        public void ModulePathMustBeApplicationRelative()
        {
            Assert.Throws<ArgumentException>(delegate
            {
                new Module("fail");
            });
        }

        [Fact]
        public void ContainsPathOfAssetInModule_ReturnsTrue()
        {
            var module = new Module("~/test");
            var asset = new Mock<IAsset>();
            asset.Setup(a => a.Accept(It.IsAny<IAssetVisitor>()))
                 .Callback<IAssetVisitor>(v => v.Visit(asset.Object));
            asset.Setup(a => a.SourceFilename).Returns("~/test/asset.js");
            module.Assets.Add(asset.Object);

            module.ContainsPath("~\\test\\asset.js").ShouldBeTrue();
        }

        [Fact]
        public void ContainsPathOfAssetInModuleWithForwardSlash_ReturnsTrue()
        {
            var module = new Module("~/test");
            var asset = new Mock<IAsset>();
            asset.Setup(a => a.Accept(It.IsAny<IAssetVisitor>()))
                 .Callback<IAssetVisitor>(v => v.Visit(asset.Object));
            asset.Setup(a => a.SourceFilename).Returns("~/test/asset.js");
            module.Assets.Add(asset.Object);

            module.ContainsPath("~/test/asset.js").ShouldBeTrue();
        }

        [Fact]
        public void ContainsPathOfAssetInModuleWithDifferentCasing_ReturnsTrue()
        {
            var module = new Module("~/test");
            var asset = new Mock<IAsset>();
            asset.Setup(a => a.Accept(It.IsAny<IAssetVisitor>()))
                 .Callback<IAssetVisitor>(v => v.Visit(asset.Object));
            asset.Setup(a => a.SourceFilename).Returns("~/test/asset.js");
            module.Assets.Add(asset.Object);

            module.ContainsPath("~\\TEST\\ASSET.js").ShouldBeTrue();
        }

        [Fact]
        public void ContainsPathOfAssetNotInModule_ReturnsFalse()
        {
            var module = new Module("~/test");

            module.ContainsPath("~\\test\\not-in-module.js").ShouldBeFalse();
        }

        [Fact]
        public void ContainsPathOfJustTheModuleItself_ReturnsTrue()
        {
            var module = new Module("~/test");

            module.ContainsPath("~/test").ShouldBeTrue();
        }

        [Fact]
        public void ContainsPathOfJustTheModuleItselfWithBackSlashes_ReturnsTrue()
        {
            var module = new Module("~/test");

            module.ContainsPath("~\\test").ShouldBeTrue();
        }

        [Fact]
        public void ContainsPathOfJustTheModuleItselfWithDifferentCasing_ReturnsTrue()
        {
            var module = new Module("~/test");

            module.ContainsPath("~\\TEST").ShouldBeTrue();
        }

        [Fact]
        public void ContainsPathOfJustTheModuleItselfWithTrailingSlash_ReturnsTrue()
        {
            var module = new Module("~/test");

            module.ContainsPath("~\\test\\").ShouldBeTrue();
        }

        [Fact]
        public void FindAssetByPathReturnsAssetWithMatchingFilename()
        {
            var module = new Module("~/test");
            var asset = new Mock<IAsset>();
            asset.Setup(a => a.SourceFilename).Returns("~/test/asset.js");
            module.Assets.Add(asset.Object);

            module.FindAssetByPath("~/test/asset.js").ShouldBeSameAs(asset.Object);
        }

        [Fact]
        public void WhenFindAssetByPathNotFound_ThenNullReturned()
        {
            var module = new Module("~/test");

            module.FindAssetByPath("~/test/notfound.js").ShouldBeNull();
        }

        [Fact]
        public void GivenAssetInSubDirectory_WhenFindAssetByPathWithBackSlashes_ThenAssetWithMatchingFilenameIsReturned()
        {
            var module = new Module("~/test");
            var asset = new Mock<IAsset>();
            asset.Setup(a => a.SourceFilename).Returns("~/test/sub/asset.js");
            module.Assets.Add(asset.Object);

            module.FindAssetByPath("~\\test\\sub\\asset.js").ShouldBeSameAs(asset.Object);
        }

        [Fact]
        public void GivenAssetInSubDirectory_WhenFindAssetByPath_ThenAssetWithMatchingFilenameIsReturned()
        {
            var module = new Module("~/test");
            var asset = new Mock<IAsset>();
            asset.Setup(a => a.SourceFilename).Returns("~/test/sub/asset.js");
            module.Assets.Add(asset.Object);

            module.FindAssetByPath("~/test/sub/asset.js").ShouldBeSameAs(asset.Object);
        }

        [Fact]
        public void AcceptCallsVisitOnVistor()
        {
            var visitor = new Mock<IAssetVisitor>();
            var module = new Module("~/test");

            module.Accept(visitor.Object);

            visitor.Verify(v => v.Visit(module));
        }

        [Fact]
        public void AcceptCallsAcceptForEachAsset()
        {
            var visitor = new Mock<IAssetVisitor>();
            var module = new Module("~/test");
            var asset1 = new Mock<IAsset>();
            var asset2 = new Mock<IAsset>();
            module.Assets.Add(asset1.Object);
            module.Assets.Add(asset2.Object);
            
            module.Accept(visitor.Object);

            asset1.Verify(a => a.Accept(visitor.Object));
            asset2.Verify(a => a.Accept(visitor.Object));
        }

        [Fact]
        public void HashIsHashOfFirstAsset()
        {
            var module = new Module("~/test");
            var asset = new Mock<IAsset>();
            asset.SetupGet(a => a.Hash).Returns(new byte[] {1, 2, 3});
            module.Assets.Add(asset.Object);
            module.Hash.SequenceEqual(new byte[] {1, 2, 3}).ShouldBeTrue();
        }

        [Fact]
        public void HashThrowsInvalidOperationExceptionWhenMoreThanOneAsset()
        {
            var module = new Module("~/test");
            module.Assets.Add(Mock.Of<IAsset>());
            module.Assets.Add(Mock.Of<IAsset>());
            Assert.Throws<InvalidOperationException>(delegate
            {
                var temp = module.Hash;
            });
        }

        [Fact]
        public void DisposeDisposesAllDisposableAssets()
        {
            var module = new Module("~");
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

    public class Module_AddReferences_Tests
    {
        [Fact]
        public void StoresReferences()
        {
            var module = new Module("~/module");
            module.AddReferences(new[] { "~\\test", "~\\other" });
            module.References.SequenceEqual(new[] { "~/test", "~/other" }).ShouldBeTrue();
        }

        [Fact]
        public void ReferenceStartingWithSlashIsConvertedToAppRelative()
        {
            var module = new Module("~/module");
            module.AddReferences(new[] { "/test" });
            module.References.Single().ShouldEqual("~/test");
        }

        [Fact]
        public void ModuleRelativePathIsConvertedToAppRelative()
        {
            var module = new Module("~/module");
            module.AddReferences(new[] { "../lib" });
            module.References.Single().ShouldEqual("~/lib");
        }

        [Fact]
        public void TrailingSlashIsRemoved()
        {
            var module = new Module("~/module");
            module.AddReferences(new[] { "../lib/" });
            module.References.Single().ShouldEqual("~/lib");
        }

        [Fact]
        public void UrlIsNotConverted()
        {
            var module = new Module("~/module");
            module.AddReferences(new[] { "http://test.com/" });
            module.References.Single().ShouldEqual("http://test.com/");
        }
    }
}
