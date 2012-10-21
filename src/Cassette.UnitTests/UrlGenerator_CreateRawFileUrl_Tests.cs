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
            url.ShouldEqual("cassette.axd/file/test-040f06fd774092478d450774f5ba30c5da78acc8.png");
        }

        [Fact]
        public void ConvertsToForwardSlashes()
        {
            SourceDirectory.Add("~\\test\\foo.png", "content");
            var url = UrlGenerator.CreateRawFileUrl("~\\test\\foo.png");
            url.ShouldEqual("cassette.axd/file/test/foo-040f06fd774092478d450774f5ba30c5da78acc8.png");
        }

        [Fact]
        public void ToleratesFilenameWithoutExtension()
        {
            SourceDirectory.Add("~\\test\\foo", "content");
            var url = UrlGenerator.CreateRawFileUrl("~\\test\\foo");
            url.ShouldEqual("cassette.axd/file/test/foo-040f06fd774092478d450774f5ba30c5da78acc8");
        }

        [Fact]
        public void InsertsHashBeforeLastPeriod()
        {
            SourceDirectory.Add("~\\test\\foo.bar.png", "content");
            var url = UrlGenerator.CreateRawFileUrl("~\\test\\foo.bar.png");
            url.ShouldEqual("cassette.axd/file/test/foo.bar-040f06fd774092478d450774f5ba30c5da78acc8.png");
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