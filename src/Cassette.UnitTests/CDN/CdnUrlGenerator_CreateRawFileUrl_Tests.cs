using System;
using Should;
using Xunit;

namespace Cassette.CDN
{
    public class CdnUrlGenerator_CreateRawFileUrl_Tests : CdnUrlGeneratorTestsBase
    {
        [Fact]
        public void CreateRawFileUrlReturnsUrlWithRoutePrefixAndHashAndPathWithoutTilde()
        {
            SourceDirectory.Add("~\\test.png", "content");
            var url = CdnUrlGenerator.CreateRawFileUrl("~/test.png");
            url.ShouldEqual(CdnTestUrl + "/" + "test.png" + "?" + "040f06fd774092478d450774f5ba30c5da78acc8");
        }

        [Fact]
        public void ConvertsToForwardSlashes()
        {
            SourceDirectory.Add("~\\test\\foo.png", "content");
            var url = CdnUrlGenerator.CreateRawFileUrl("~\\test\\foo.png");
            url.ShouldEqual(CdnTestUrl + "/" + "test/foo.png" + "?" + "040f06fd774092478d450774f5ba30c5da78acc8");
        }

        [Fact]
        public void ToleratesFilenameWithoutExtension()
        {
            SourceDirectory.Add("~\\test\\foo", "content");
            var url = CdnUrlGenerator.CreateRawFileUrl("~\\test\\foo");
            url.ShouldEqual(CdnTestUrl + "/" + "test/foo" + "?" + "040f06fd774092478d450774f5ba30c5da78acc8");
        }

        [Fact]
        public void ToleratesComplexFilename()
        {
            SourceDirectory.Add("~\\test\\foo.bar.png", "content");
            var url = CdnUrlGenerator.CreateRawFileUrl("~\\test\\foo.bar.png");
            url.ShouldEqual(CdnTestUrl + "/" + "test/foo.bar.png" + "?" + "040f06fd774092478d450774f5ba30c5da78acc8");
        }

        [Fact]
        public void EscapesSpaces()
        {
            SourceDirectory.Add("~\\space test.png", "content");
            var url = CdnUrlGenerator.CreateRawFileUrl("~/space test.png");
            url.ShouldEqual(CdnTestUrl + "/" + "space%20test.png" + "?" + "040f06fd774092478d450774f5ba30c5da78acc8");
        }

        [Fact]
        public void ArgumentExceptionThrownWhenFilenameDoesNotStartWithTilde()
        {
            Assert.Throws<ArgumentException>(delegate
            {
                CdnUrlGenerator.CreateRawFileUrl("fail.png");
            });
        }
    }
}