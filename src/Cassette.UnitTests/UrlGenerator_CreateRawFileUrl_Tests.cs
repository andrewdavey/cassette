using System;
using Should;
using Xunit;

namespace Cassette
{
    public class UrlGenerator_CreateRawFileUrl_Tests : UrlGeneratorTestsBase
    {
        [Fact]
        public void CreateRawFileUrlReturnsUrlWithRoutePrefixAndPathWithoutTildeAndHash()
        {
            var url = UrlGenerator.CreateRawFileUrl("~/test.png", "hash");
            url.ShouldEqual("_cassette/file/test_hash_png");
        }

        [Fact]
        public void ConvertsToForwardSlashes()
        {
            var url = UrlGenerator.CreateRawFileUrl("~\\test\\foo.png", "hash");
            url.ShouldEqual("_cassette/file/test/foo_hash_png");
        }

        [Fact]
        public void ArgumentExceptionThrownWhenFilenameDoesNotStartWithTilde()
        {
            Assert.Throws<ArgumentException>(delegate
            {
                UrlGenerator.CreateRawFileUrl("fail.png", "hash");
            });
        }
    }
}