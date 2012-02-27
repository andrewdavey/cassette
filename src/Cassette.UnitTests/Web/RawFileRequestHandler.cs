﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Security.Cryptography;
using Moq;
using Xunit;

namespace Cassette.Web
{
    public class RawFileRequestHandler_Tests
    {
        readonly IEnumerable<Bundle> bundles;

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
            bundles = new[] { bundle };
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

                http.MapRoute("{*path}", c => new RawFileRequestHandler(c, bundles));
                http.Server.Setup(s => s.MapPath("~/test.png")).Returns(filename);


                http.Get("~/test_hash.png");


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

                http.MapRoute("{*path}", c => new RawFileRequestHandler(c, bundles));
                http.Server.Setup(s => s.MapPath("~/test.png")).Returns(filename);

                string eTag;
                using (var hasher = SHA1.Create())
                {
                    eTag = "\"" + Convert.ToBase64String(hasher.ComputeHash(content)) + "\"";
                }

                http.RequestHeaders["If-None-Match"] = eTag;
                http.Get("~/test_hash.png");

                http.Response.VerifySet(r => r.StatusCode = 304);
            }
        }

        [Fact]
        public void WhenRequestFileWithDotExtension_ThenRequestIsResolvedSuccessfully()
        {
            using (var temp = new TempDirectory())
            using (var http = new HttpTestHarness())
            {
                var filename = Path.Combine(temp, "test.png");
                var content = new byte[] { 1, 2, 3 };
                File.WriteAllBytes(filename, content);

                http.MapRoute("{*path}", c => new RawFileRequestHandler(c, bundles));
                http.Server.Setup(s => s.MapPath("~/test.png")).Returns(filename);

                http.Get("~/test_hash.png");
                http.Response.Verify(r => r.WriteFile(filename));
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

                http.MapRoute("{*path}", c => new RawFileRequestHandler(c, bundles));
                http.Server.Setup(s => s.MapPath("~/protected.png")).Returns(filename);

                http.Get("~/protected_hash.png");

                http.Response.Verify(r => r.WriteFile(It.IsAny<string>()), Times.Never());
                http.Response.VerifySet(r => r.StatusCode = 404);
            }
        }
    }
}