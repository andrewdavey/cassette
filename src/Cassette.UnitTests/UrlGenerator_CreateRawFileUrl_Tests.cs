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
            url.ShouldEqual("file/test-hash.png");
        }

        [Fact]
        public void ConvertsToForwardSlashes()
        {
            var url = UrlGenerator.CreateRawFileUrl("~\\test\\foo.png", "hash");
            url.ShouldEqual("file/test/foo-hash.png");
        }

        [Fact]
        public void ToleratesFilenameWithoutExtension()
        {
            var url = UrlGenerator.CreateRawFileUrl("~\\test\\foo", "hash");
            url.ShouldEqual("file/test/foo-hash");
        }

        [Fact]
        public void InsertsHashBeforeLastPeriod()
        {
            var url = UrlGenerator.CreateRawFileUrl("~\\test\\foo.bar.png", "hash");
            url.ShouldEqual("file/test/foo.bar-hash.png");
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