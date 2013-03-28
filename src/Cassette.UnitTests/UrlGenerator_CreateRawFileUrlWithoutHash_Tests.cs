using System;
using Should;
using Xunit;

namespace Cassette
{
    public class UrlGenerator_CreateRawFileUrlWithoutHash_Tests : UrlGeneratorTestsBase
    {
        public UrlGenerator_CreateRawFileUrlWithoutHash_Tests() : base(true)
        {
        }

        [Fact]
        public void CreateRawFileUrlReturnsUrlWithRoutePrefixAndHashAndPathWithoutTilde()
        {
            SourceDirectory.Add("~\\test.png", "content");
            var url = UrlGenerator.CreateRawFileUrl("~/test.png");
            url.ShouldEqual("cassette.axd/file/test.png");
        }

        [Fact]
        public void ConvertsToForwardSlashes()
        {
            SourceDirectory.Add("~\\test\\foo.png", "content");
            var url = UrlGenerator.CreateRawFileUrl("~\\test\\foo.png");
            url.ShouldEqual("cassette.axd/file/test/foo.png");
        }

        [Fact]
        public void ToleratesFilenameWithoutExtension()
        {
            SourceDirectory.Add("~\\test\\foo", "content");
            var url = UrlGenerator.CreateRawFileUrl("~\\test\\foo");
            url.ShouldEqual("cassette.axd/file/test/foo");
        }

        [Fact]
        public void InsertsHashBeforeLastPeriod()
        {
            SourceDirectory.Add("~\\test\\foo.bar.png", "content");
            var url = UrlGenerator.CreateRawFileUrl("~\\test\\foo.bar.png");
            url.ShouldEqual("cassette.axd/file/test/foo.bar.png");
        }

        [Fact]
        public void EscapesSpaces()
        {
            SourceDirectory.Add("~\\space test.png", "content");
            var url = UrlGenerator.CreateRawFileUrl("~/space test.png");
            url.ShouldEqual("cassette.axd/file/space%20test.png");
        }
    }
}