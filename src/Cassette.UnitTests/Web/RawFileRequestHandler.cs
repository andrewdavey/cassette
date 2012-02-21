using System;
using System.IO;
using System.Security.Cryptography;
using Moq;
using Xunit;

namespace Cassette.Web
{
    public class RawFileRequestHandler_Tests
    {
        readonly BundleContainer container;

        public RawFileRequestHandler_Tests()
        {
            var bundle = new TestableBundle("~");
            var asset = new Mock<IAsset>();
            asset.Setup(a => a.Accept(It.IsAny<IBundleVisitor>()))
                .Callback<IBundleVisitor>(v => v.Visit(asset.Object));
            asset.SetupGet(a => a.References).Returns(new[]
            {
                new AssetReference("~/test.png", asset.Object, -1, AssetReferenceType.RawFilename)
            });
            bundle.Assets.Add(asset.Object);
            container = new BundleContainer(new[] { bundle });
        }

        [Fact]
        public void WhenRequestFile_ThenResponseETagHeaderIsSHA1OfContents()
        {
            using (var temp = new TempDirectory())
            using (var http = new HttpTestHarness())
            {
                var filename = Path.Combine(temp, "test.png");
                var content = new byte[] { 1, 2, 3 };
                File.WriteAllBytes(filename, content);

                http.MapRoute("{*path}", c => new RawFileRequestHandler(c, () => container));
                http.Server.Setup(s => s.MapPath("~/test.png")).Returns(filename);


                http.Get("~/test_hash_png");


                string expectedETag;
                using (var hasher = SHA1.Create())
                {
                    expectedETag = "\"" + Convert.ToBase64String(hasher.ComputeHash(content)) + "\"";
                }
                http.ResponseCache.Verify(c => c.SetETag(expectedETag));
            }
        }

        [Fact]
        public void WhenRequestWithETagThatMatchesCurrent_ThenNotModifiedResponseReturned()
        {
            using (var temp = new TempDirectory())
            using (var http = new HttpTestHarness())
            {
                var filename = Path.Combine(temp, "test.png");
                var content = new byte[] { 1, 2, 3 };
                File.WriteAllBytes(filename, content);

                http.MapRoute("{*path}", c => new RawFileRequestHandler(c, () => container));
                http.Server.Setup(s => s.MapPath("~/test.png")).Returns(filename);

                string eTag;
                using (var hasher = SHA1.Create())
                {
                    eTag = "\"" + Convert.ToBase64String(hasher.ComputeHash(content)) + "\"";
                }

                http.RequestHeaders["If-None-Match"] = eTag;
                http.Get("~/test_hash_png");

                http.Response.VerifySet(r => r.StatusCode = 304);
            }
        }

        [Fact]
        public void WhenRequestFileThatIsNoReferencedByAsset_ThenDoNotReturnFile()
        {
            using (var temp = new TempDirectory())
            using (var http = new HttpTestHarness())
            {
                var filename = Path.Combine(temp, "protected.png");
                var content = new byte[] { 1, 2, 3 };
                File.WriteAllBytes(filename, content);

                http.MapRoute("{*path}", c => new RawFileRequestHandler(c, () => container));
                http.Server.Setup(s => s.MapPath("~/protected.png")).Returns(filename);

                http.Get("~/protected_hash_png");

                http.Response.Verify(r => r.WriteFile(It.IsAny<string>()), Times.Never());
                http.Response.VerifySet(r => r.StatusCode = 404);
            }
        }
    }
}