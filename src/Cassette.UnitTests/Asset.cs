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
using System.Security.Cryptography;
using Cassette.IO;
using Cassette.Utilities;
using Moq;
using Should;
using Xunit;

namespace Cassette
{
    public class Asset_Tests : IDisposable
    {
        public Asset_Tests()
        {
            root = Directory.CreateDirectory(Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString()));
            root.CreateSubdirectory("bundle");
            
            bundle = new Bundle("~/bundle");
            asset = new Asset("~/bundle/test.js", bundle, StubFile("asset content"));
            bundle.Assets.Add(asset);

            var another = new Asset("~/bundle/another.js", bundle, StubFile());
            bundle.Assets.Add(another);
        }

        readonly Asset asset;
        readonly DirectoryInfo root;
        readonly Bundle bundle;

        IFile StubFile(string content = "")
        {
            var file = new Mock<IFile>();
            file.Setup(f => f.Open(It.IsAny<FileMode>(), It.IsAny<FileAccess>(), FileShare.ReadWrite))
                .Returns(() => content.AsStream());
            return file.Object;
        }

        [Fact]
        public void ConstructorNormalizesPath()
        {
            root.CreateSubdirectory("bundle\\test");
            File.WriteAllText(Path.Combine(root.FullName, "bundle", "test", "bar.js"), "");
            var asset = new Asset("~\\bundle\\test\\bar.js", bundle, StubFile());
            asset.SourceFilename.ShouldEqual("~/bundle/test/bar.js");
        }

        [Fact]
        public void OpenStream_OpensTheFile()
        {
            using (var stream = asset.OpenStream())
            {
                using (var reader = new StreamReader(stream))
                {
                    reader.ReadToEnd().ShouldEqual("asset content");
                }
            }
        }

        [Fact]
        public void Hash_IsSHA1OfTheFileContent()
        {
            byte[] expectedHash;
            using (var sha1 = SHA1.Create())
            using (var fileStream = "asset content".AsStream())
            {
                expectedHash = sha1.ComputeHash(fileStream);
            }

            asset.Hash.SequenceEqual(expectedHash).ShouldBeTrue();
        }

        [Fact]
        public void WhenAddAssetTransformer_ThenHasTransformersIsTrue()
        {
            asset.AddAssetTransformer(Mock.Of<IAssetTransformer>());

            asset.HasTransformers.ShouldBeTrue();
        }

        [Fact]
        public void WhenAddAssetTransformer_ThenOpenStreamReturnsTransformedStream()
        {
            Stream transformedStream = null;
            var transformer = new Mock<IAssetTransformer>();
            transformer.Setup(t => t.Transform(It.IsAny<Func<Stream>>(), asset))
                        .Returns(() => () => transformedStream = new MemoryStream());
            
            asset.AddAssetTransformer(transformer.Object);

            using (var stream = asset.OpenStream())
            {
                stream.ShouldBeSameAs(transformedStream);
            }
        }

        [Fact]
        public void WhenAddAssetTransformerCalledTwice_ThenOpenStreamReturnsTwiceTransformedStream()
        {
            var transformer1 = new Mock<IAssetTransformer>();
            var transformer2 = new Mock<IAssetTransformer>();
            Func<Stream> openStream1 = () => Stream.Null;
            var stream2 = Mock.Of<Stream>();
            Func<Stream> openStream2 = () => stream2;
            transformer1.Setup(t => t.Transform(It.IsAny<Func<Stream>>(), asset))
                        .Returns(() => openStream1).Verifiable();
            transformer2.Setup(t => t.Transform(openStream1, asset))
                        .Returns(() => openStream2).Verifiable();

            asset.AddAssetTransformer(transformer1.Object);
            asset.AddAssetTransformer(transformer2.Object);
            
            using (var result = asset.OpenStream())
            {
                result.ShouldBeSameAs(stream2);
            }
        }

        [Fact]
        public void AddReferenceToSiblingFilename_ExpandsFilenameToAbsolutePath()
        {
            asset.AddReference("another.js", 1);

            asset.References.First().Path.ShouldEqual("~/bundle/another.js");
        }

        [Fact]
        public void AddReferenceToSiblingFilenameInSubDirectory_ExpandsFilenameToAbsolutePath()
        {
            root.CreateSubdirectory("bundle\\sub");
            File.WriteAllText(Path.Combine(root.FullName, "bundle", "sub", "another.js"), "");
            var another = new Asset("~/bundle/sub/another.js", bundle, StubFile());
            bundle.Assets.Add(another);

            asset.AddReference("sub\\another.js", 1);

            asset.References.First().Path.ShouldEqual("~/bundle/sub/another.js");
        }

        [Fact]
        public void AddReferenceToSiblingFilename_AssignsLineNumber()
        {
            asset.AddReference("another.js", 1);

            asset.References.First().SourceLineNumber.ShouldEqual(1);
        }

        [Fact]
        public void AddReferenceToSiblingFilename_CreatesSameBundleReference()
        {
            asset.AddReference("another.js", 1);

            asset.References.First().Type.ShouldEqual(AssetReferenceType.SameBundle);
        }

        [Fact]
        public void AddReferenceToAssetInAnotherBundle_ExpandsFilenameToAbsolutePath()
        {
            asset.AddReference("../another/test.js", 1);

            asset.References.First().Path.ShouldEqual("~/another/test.js");
        }

        [Fact]
        public void AddReferenceToAssetInAnotherBundle_CreatesDifferentBundleReference()
        {
            asset.AddReference("../another/test.js", 1);

            asset.References.First().Type.ShouldEqual(AssetReferenceType.DifferentBundle);
        }

        [Fact]
        public void AddReferenceToAssetWithPathAbsoluteToWebApplication()
        {
            asset.AddReference("/another/test.js", 1);

            var reference = asset.References.First();
            reference.Path.ShouldEqual("~/another/test.js");
            reference.Type.ShouldEqual(AssetReferenceType.DifferentBundle);
        }

        [Fact]
        public void WhenAddReferenceToAssetPathStartingWithTilde_ThenPathIsConvertedToAppRelative()
        {
            asset.AddReference("~/another/test.js", 1);

            var reference = asset.References.First();
            reference.Path.ShouldEqual("~/another/test.js");
        }

        [Fact]
        public void AddReferenceToAssetThatIsNotInSameBundleThrowsAssetReferenceException()
        {
            Assert.Throws<AssetReferenceException>(delegate
            {
                asset.AddReference("not-in-bundle.js", 1);
            });
        }

        [Fact]
        public void AddRawFileReferenceNormalizesPathToBeAppRelative()
        {
            asset.AddRawFileReference("../test.png");

            var reference = asset.References.First();
            reference.Path.ShouldEqual("~/test.png");
        }

        [Fact]
        public void WhenAddRawReferenceTwiceWithSamePath_ThenReferencesHasItOnlyOnce()
        {
            asset.AddRawFileReference("test.png");
            asset.AddRawFileReference("test.png");

            asset.References.Count().ShouldEqual(1);
        }

        [Fact]
        public void WhenAddRawReferenceTwiceWithSameEffectivePath_ThenReferencesHasItOnlyOnce()
        {
            asset.AddRawFileReference("../bundle/test.png");
            asset.AddRawFileReference("./test.png");

            asset.References.Count().ShouldEqual(1);
        }

        [Fact]
        public void WhenAddReferenceToUrl_ThenReferenceIsDifferentBundle()
        {
            var url = "http://maps.google.com/maps/api/js?v=3.2&sensor=false";
            asset.AddReference(url, 1);
            asset.References.First().Path.ShouldEqual(url);
            asset.References.First().Type.ShouldEqual(AssetReferenceType.Url);
        }

        [Fact]
        public void WhenAddReferenceToHttpsUrl_ThenReferenceIsDifferentBundle()
        {
            var url = "https://maps.google.com/maps/api/js?v=3.2&sensor=false";
            asset.AddReference(url, 1);
            asset.References.First().Path.ShouldEqual(url);
            asset.References.First().Type.ShouldEqual(AssetReferenceType.Url);
        }

        [Fact]
        public void WhenAddReferenceToProtocolRelativeUrl_ThenReferenceIsDifferentBundle()
        {
            var url = "//maps.google.com/maps/api/js?v=3.2&sensor=false";
            asset.AddReference(url, 1);
            asset.References.First().Path.ShouldEqual(url);
            asset.References.First().Type.ShouldEqual(AssetReferenceType.Url);
        }

        [Fact]
        public void AcceptCallsVisitOnVisitor()
        {
            var visitor = new Mock<IAssetVisitor>();
            asset.Accept(visitor.Object);
            visitor.Verify(v => v.Visit(asset));
        }

        void IDisposable.Dispose()
        {
            root.Delete(true);
        }
    }

    public class Asset_CreateCacheManifest_Tests : IDisposable
    {
        public Asset_CreateCacheManifest_Tests()
        {
            root = Directory.CreateDirectory(Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString()));
            root.CreateSubdirectory("bundle");
            filename = Path.Combine(root.FullName, "bundle", "test.js");
            // Write some testable content to the file.
            File.WriteAllText(filename, "asset content");
            var fileSystem = new FileSystemDirectory(root.FullName);

            var bundle = new Bundle("~/bundle");
            asset = new Asset("~/bundle/test.js", bundle, fileSystem.GetFile("bundle\\test.js"));
            bundle.Assets.Add(asset);

            File.WriteAllText(Path.Combine(root.FullName, "bundle", "another.js"), "");
            var another = new Asset("~/bundle/another.js", bundle, fileSystem.GetFile("bundle\\another.js"));
            bundle.Assets.Add(another);
        }

        readonly string filename;
        readonly Asset asset;
        readonly DirectoryInfo root;

        [Fact]
        public void CreateCacheManifestReturnsSingleXElement()
        {
            var element = asset.CreateCacheManifest().Single();
            element.Name.LocalName.ShouldEqual("Asset");
        }

        [Fact]
        public void GivenAssetHasReference_ThenXElementHasReferenceChildElement()
        {
            asset.AddReference("another.js", 1);
            var element = asset.CreateCacheManifest().Single();
            element.Element("Reference").ShouldNotBeNull();
        }

        public void Dispose()
        {
            root.Delete(true);
        }
    }
}

