using System;
using System.IO;
using System.Security.Cryptography;
using Xunit;

namespace Cassette.Web
{
    class RawFileRequestHandler_Tests
    {
        [Fact]
        public void WhenRequestFile_ThenResponseETagHeaderIsSHA1OfContents()
        {
            using (var temp = new TempDirectory())
            using (var http = new HttpTestHarness())
            {
                var filename = Path.Combine(temp, "test.png");
                var content = new byte[] { 1, 2, 3 };
                File.WriteAllBytes(filename, content);
                
                http.MapRoute("{*path}", c => new RawFileRequestHandler(c));
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

                http.MapRoute("{*path}", c => new RawFileRequestHandler(c));
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
    }
}