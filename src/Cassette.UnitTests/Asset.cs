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
            
            bundle = new TestableBundle("~/bundle");
            sourceFile = StubFile("asset content");
            asset = new FileAsset(sourceFile, bundle);
            bundle.Assets.Add(asset);

            var another = new FileAsset(StubFile(fullPath: "~/bundle/another.js"), bundle);
            bundle.Assets.Add(another);
        }

        readonly FileAsset asset;
        readonly DirectoryInfo root;
        readonly Bundle bundle;
        readonly IFile sourceFile;

        IFile StubFile(string content = "", string fullPath = null)
        {
            var file = new Mock<IFile>();
            var directory = new Mock<IDirectory>();
            directory.SetupGet(d => d.FullPath).Returns("~/bundle");
            file.SetupGet(f => f.Directory).Returns(directory.Object);
            file.SetupGet(f => f.FullPath).Returns(fullPath ?? "~/bundle/asset.js");
            file.Setup(f => f.Open(It.IsAny<FileMode>(), It.IsAny<FileAccess>(), FileShare.ReadWrite))
                .Returns(() => content.AsStream());
            return file.Object;
        }

        [Fact]
        public void SourceFileIsPassedViaConstructor()
        {
            asset.SourceFile.ShouldBeSameAs(sourceFile);
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
        public void Hash_IsSHA1OfTheTransformedAssetContent()
        {
            var transformer = new Mock<IAssetTransformer>();
            transformer
                .Setup(t => t.Transform(It.IsAny<Func<Stream>>(), asset))
                .Returns(() => new MemoryStream(new byte[] { 1, 2, 3 }));
            asset.AddAssetTransformer(transformer.Object);

            using (var sha1 = SHA1.Create())
            {
                var expected = sha1.ComputeHash(new byte[] { 1, 2, 3 });
                asset.Hash.ShouldEqual(expected);
            }
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
            File.WriteAllText(PathUtilities.Combine(root.FullName, "bundle", "sub", "another.js"), "");
            var another = new FileAsset(StubFile(fullPath: "~/bundle/sub/another.js"), bundle);
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
        public void AddReferenceToAssetInAnotherBundleWithShorterPath_CreatesDifferentBundleReference()
        {
            asset.AddReference("~/a.js", 1);

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
        public void WhenAddRawFileReferenceStartingWithForwardSlash_ThenReferencePathhasTildePrefixAdded()
        {
            asset.AddRawFileReference("/test.png");
            asset.References.First().Path.ShouldEqual("~/test.png");
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
            var visitor = new Mock<IBundleVisitor>();
            asset.Accept(visitor.Object);
            visitor.Verify(v => v.Visit(asset));
        }

        void IDisposable.Dispose()
        {
            root.Delete(true);
        }
    }
}