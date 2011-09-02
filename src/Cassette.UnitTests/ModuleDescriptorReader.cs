using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Cassette.IO;
using Cassette.Utilities;
using Should;
using Xunit;
using Moq;

namespace Cassette
{
    public class ModuleDescriptorReader_Tests
    {
        readonly List<string> files = new List<string>();
        
        ModuleDescriptorReader GetReader(string descriptor)
        {
            var source = new Mock<IFile>();
            source
                .Setup(s => s.Open(FileMode.Open, FileAccess.Read))
                .Returns(() => descriptor.AsStream());

            return new ModuleDescriptorReader(source.Object, files);
        }

        void FilesExist(params string[] result)
        {
            files.AddRange(result);
        }

        [Fact]
        public void SkipsBlankLines()
        {
            var reader = GetReader("\r\n\t\n ");
            var result = reader.Read();
            result.AssetFilenames.ShouldBeEmpty();
        }

        [Fact]
        public void SkipsComments()
        {
            var reader = GetReader("#comment");
            var result = reader.Read();
            result.AssetFilenames.ShouldBeEmpty();
        }

        [Fact]
        public void SkipsCommentsWithLeadingWhitespace()
        {
            var reader = GetReader("#comment with leading space");
            var result = reader.Read();
            result.AssetFilenames.ShouldBeEmpty();
        }

        [Fact]
        public void ReturnsFilesSpecified()
        {
            FilesExist("test1.js", "test2.js");
            var reader = GetReader("test1.js\ntest2.js");
            var result = reader.Read();
            result.AssetFilenames.SequenceEqual(new[] { "test1.js", "test2.js" }).ShouldBeTrue();
        }

        [Fact]
        public void ThrowsExceptionWhenFileNotFound()
        {
            FilesExist("test1.js");
            var reader = GetReader("test1.js\ntest2.js");
            Assert.Throws<FileNotFoundException>(delegate
            {
                reader.Read();
            });
        }

        [Fact]
        public void CommentAfterFilenameIgnored()
        {
            FilesExist("test.js");
            var reader = GetReader("test.js # comment");
            var result = reader.Read();
            result.AssetFilenames.SequenceEqual(new[] { "test.js" }).ShouldBeTrue();
        }

        [Fact]
        public void AsteriskIncludesAllFiles()
        {
            FilesExist("test1.js", "test2.js");
            var reader = GetReader("*");
            var result = reader.Read();
            result.AssetFilenames.SequenceEqual(new[] { "test1.js", "test2.js" }).ShouldBeTrue();
        }

        [Fact]
        public void AsteriskIncludesAllFilesNotAlreadyadded()
        {
            FilesExist("test1.js", "test2.js", "test3.js");
            var reader = GetReader("test2.js\n*");
            var result = reader.Read();
            result.AssetFilenames.SequenceEqual(new[] { "test2.js", "test1.js", "test3.js" }).ShouldBeTrue();
        }

        [Fact]
        public void GivenAssetsSectionReadReturnsAssetFilenames()
        {
            FilesExist("test.js");
            var reader = GetReader("[assets]\ntest.js");
            var result = reader.Read();
            result.AssetFilenames.SequenceEqual(new[] { "test.js" }).ShouldBeTrue();
        }

        [Fact]
        public void GivenAssetsSectionWithTrailingCommentReadReturnsAssetFilenames()
        {
            FilesExist("test.js");
            var reader = GetReader("[assets]#comment\ntest.js");
            var result = reader.Read();
            result.AssetFilenames.SequenceEqual(new[] { "test.js" }).ShouldBeTrue();
        }

        [Fact]
        public void GivenReferencesSectionReadReturnsReferences()
        {
            var reader = GetReader("[references]\n../lib/other.js");
            var result = reader.Read();
            result.References.SequenceEqual(new[]{"../lib/other.js"}).ShouldBeTrue();
        }

        [Fact]
        public void GivenBothAssetsAndReferencesSectionsReadReturnsBoth()
        {
            FilesExist("test.js");
            var reader = GetReader("[assets]\ntest.js\n[references]\n../lib/other.js");
            var result = reader.Read();
            result.AssetFilenames.SequenceEqual(new[] { "test.js" }).ShouldBeTrue();
            result.References.SequenceEqual(new[] { "../lib/other.js" }).ShouldBeTrue();
        }

        [Fact]
        public void GivenExternalSectionWithUrl_WhenRead_ThenResultHasExternalUrl()
        {
            var reader = GetReader("[external]\nurl = http://test.com/api.js");
            var result = reader.Read();
            result.ExternalUrl.ShouldEqual("http://test.com/api.js");
        }

        [Fact]
        public void GivenExternalSectionWithNonUrlContent_WhenRead_ThenThrowException()
        {
            var reader = GetReader("[external]\nurl = fail");
            Assert.Throws<Exception>(delegate
            {
                reader.Read();
            });
        }

        [Fact]
        public void GivenExternalSectionWithMoreThanOneUrlLine_WhenRead_ThenThrowException()
        {
            var reader = GetReader("[external]\nurl = http://test.com/api1.js\nurl = http://test.com/api2.js");
            Assert.Throws<Exception>(delegate
            {
                reader.Read();
            });
        }

        [Fact]
        public void GivenFallbackConditionSection_WhenRead_ThenResultHasFallbackCondition()
        {
            var reader = GetReader("[external]\nurl = http://test.com/api.js\nfallbackCondition = !window.API");
            var result = reader.Read();
            result.FallbackCondition.ShouldEqual("!window.API");
        }

        [Fact]
        public void GivenExternalSectionWithMoreThanOneFallbackConditionLine_WhenRead_ThenThrowException()
        {
            var reader = GetReader("[external]\nurl = http://test.com/api1.js\nfallbackCondition = !window.API\nfallbackCondition = !window.API");
            Assert.Throws<Exception>(delegate
            {
                reader.Read();
            });
        }

        [Fact]
        public void GivenExternalSectionWithFallbackConditionButNoUrl_WhenRead_ThenThrowException()
        {
            var reader = GetReader("[external]\nfallbackCondition = !window.API");
            Assert.Throws<Exception>(delegate
            {
                reader.Read();
            });
        }
    }
}