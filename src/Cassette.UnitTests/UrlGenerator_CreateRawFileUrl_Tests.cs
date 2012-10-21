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
            SourceDirectory.Add("~\\test.png", "content");
            var url = UrlGenerator.CreateRawFileUrl("~/test.png");
            url.ShouldEqual("cassette.axd/file/test-BA8G_XdAkkeNRQd09bowxdp4rMg=.png");
        }

        [Fact]
        public void ConvertsToForwardSlashes()
        {
            SourceDirectory.Add("~\\test\\foo.png", "content");
            var url = UrlGenerator.CreateRawFileUrl("~\\test\\foo.png");
            url.ShouldEqual("cassette.axd/file/test/foo-BA8G_XdAkkeNRQd09bowxdp4rMg=.png");
        }

        [Fact]
        public void ToleratesFilenameWithoutExtension()
        {
            SourceDirectory.Add("~\\test\\foo", "content");
            var url = UrlGenerator.CreateRawFileUrl("~\\test\\foo");
            url.ShouldEqual("cassette.axd/file/test/foo-BA8G_XdAkkeNRQd09bowxdp4rMg=");
        }

        [Fact]
        public void InsertsHashBeforeLastPeriod()
        {
            SourceDirectory.Add("~\\test\\foo.bar.png", "content");
            var url = UrlGenerator.CreateRawFileUrl("~\\test\\foo.bar.png");
            url.ShouldEqual("cassette.axd/file/test/foo.bar-BA8G_XdAkkeNRQd09bowxdp4rMg=.png");
        }

        [Fact]
        public void ArgumentExceptionThrownWhenFilenameDoesNotStartWithTilde()
        {
            Assert.Throws<ArgumentException>(delegate
            {
                UrlGenerator.CreateRawFileUrl("fail.png");
            });
        }
    }
}