using System;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
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
            root.CreateSubdirectory("module");
            filename = Path.Combine(root.FullName, "module", "test.js");
            // Write some testable content to the file.
            File.WriteAllText(filename, "asset content");
            var fileSystem = new FileSystem(root.FullName);

            var module = new Module("module");
            asset = new Asset("test.js", module, fileSystem.GetFile("module\\test.js"));
            module.Assets.Add(asset);

            File.WriteAllText(Path.Combine(root.FullName, "module", "another.js"), "");
            var another = new Asset("another.js", module, fileSystem.GetFile("module\\another.js"));
            module.Assets.Add(another);
        }

        readonly string filename;
        readonly Asset asset;
        readonly DirectoryInfo root;

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
            using (var fileStream = File.OpenRead(filename))
            {
                expectedHash = sha1.ComputeHash(fileStream);
            }

            asset.Hash.SequenceEqual(expectedHash).ShouldBeTrue();
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

            asset.References.First().Path.ShouldEqual("~\\module\\another.js");
        }

        [Fact]
        public void AddReferenceToSiblingFilename_AssignsLineNumber()
        {
            asset.AddReference("another.js", 1);

            var expectedFilename = Path.Combine(Path.GetDirectoryName(filename), "another.js");
            asset.References.First().SourceLineNumber.ShouldEqual(1);
        }

        [Fact]
        public void AddReferenceToSiblingFilename_CreatesSameModuleReference()
        {
            asset.AddReference("another.js", 1);

            var expectedFilename = Path.Combine(Path.GetDirectoryName(filename), "another.js");
            asset.References.First().Type.ShouldEqual(AssetReferenceType.SameModule);
        }

        [Fact]
        public void AddReferenceToAssetInAnotherModule_ExpandsFilenameToAbsolutePath()
        {
            asset.AddReference("../another/test.js", 1);

            asset.References.First().Path.ShouldEqual("~\\another\\test.js");
        }

        [Fact]
        public void AddReferenceToAssetInAnotherModule_CreatesDifferentModuleReference()
        {
            asset.AddReference("../another/test.js", 1);

            asset.References.First().Type.ShouldEqual(AssetReferenceType.DifferentModule);
        }

        [Fact]
        public void AddReferenceToAssetWithPathAbsoluteToWebApplication()
        {
            asset.AddReference("/another/test.js", 1);

            var reference = asset.References.First();
            reference.Path.ShouldEqual("~\\another\\test.js");
            reference.Type.ShouldEqual(AssetReferenceType.DifferentModule);
        }

        [Fact]
        public void WhenAddReferenceToAssetPathStartingWithTilde_ThenPathIsConvertedToAppRelative()
        {
            asset.AddReference("~/another/test.js", 1);

            var reference = asset.References.First();
            reference.Path.ShouldEqual("~\\another\\test.js");
        }

        [Fact]
        public void AddReferenceToAssetThatIsNotInSameModuleThrowsAssetReferenceException()
        {
            Assert.Throws<AssetReferenceException>(delegate
            {
                asset.AddReference("not-in-module.js", 1);
            });
        }

        [Fact]
        public void AddRawFileReferenceNormalizesPathToBeAppRelative()
        {
            asset.AddRawFileReference("../test.png");

            var reference = asset.References.First();
            reference.Path.ShouldEqual("~\\test.png");
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
            asset.AddRawFileReference("../module/test.png");
            asset.AddRawFileReference("./test.png");

            asset.References.Count().ShouldEqual(1);
        }

        [Fact]
        public void WhenAddReferenceToUrl_ThenReferenceIsDifferentModule()
        {
            var url = "http://maps.google.com/maps/api/js?v=3.2&sensor=false";
            asset.AddReference(url, 1);
            asset.References.First().Path.ShouldEqual(url);
            asset.References.First().Type.ShouldEqual(AssetReferenceType.Url);
        }

        [Fact]
        public void WhenAddReferenceToHttpsUrl_ThenReferenceIsDifferentModule()
        {
            var url = "https://maps.google.com/maps/api/js?v=3.2&sensor=false";
            asset.AddReference(url, 1);
            asset.References.First().Path.ShouldEqual(url);
            asset.References.First().Type.ShouldEqual(AssetReferenceType.Url);
        }

        [Fact]
        public void WhenAddReferenceToProtocolRelativeUrl_ThenReferenceIsDifferentModule()
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
            root.CreateSubdirectory("module");
            filename = Path.Combine(root.FullName, "module", "test.js");
            // Write some testable content to the file.
            File.WriteAllText(filename, "asset content");
            var fileSystem = new FileSystem(root.FullName);

            var module = new Module("module");
            asset = new Asset("test.js", module, fileSystem.GetFile("module\\test.js"));
            module.Assets.Add(asset);

            File.WriteAllText(Path.Combine(root.FullName, "module", "another.js"), "");
            var another = new Asset("another.js", module, fileSystem.GetFile("module\\another.js"));
            module.Assets.Add(another);
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
