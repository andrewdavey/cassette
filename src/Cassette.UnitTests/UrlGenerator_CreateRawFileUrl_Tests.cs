using System;
using Should;
using Xunit;

namespace Cassette
{
    public class UrlGenerator_CreateRawFileUrl_Tests : UrlGeneratorTestsBase
    {
        [Fact]
        public void CreateRawFileUrlReturnsUrlWithRoutePrefixAndHashAndPathWithoutTilde()
        {
            var url = UrlGenerator.CreateRawFileUrl("~/test.png", "hash");
            url.ShouldEqual("file/hash/test.png");
        }

        [Fact]
        public void ConvertsToForwardSlashes()
        {
            var url = UrlGenerator.CreateRawFileUrl("~\\test\\foo.png", "hash");
            url.ShouldEqual("file/hash/test/foo.png");
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