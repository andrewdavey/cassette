using System;
using System.IO;
using System.Linq;
using Cassette.IO;
using Cassette.Utilities;
using Should;
using Xunit;
using Moq;

namespace Cassette
{
    public class BundleDescriptorReader_Tests : IDisposable
    {
        readonly TempDirectory tempDirectory;

        public BundleDescriptorReader_Tests()
        {
            tempDirectory = new TempDirectory();            
        }

        BundleDescriptorReader GetReader(string descriptor)
        {
            var file = new Mock<IFile>();
            var directory = new Mock<IDirectory>();
            file
                .Setup(s => s.Open(FileMode.Open, FileAccess.Read, FileShare.ReadWrite))
                .Returns(() => descriptor.AsStream());
            file
                .SetupGet(s => s.Directory)
                .Returns(directory.Object);
            directory
                .SetupGet(d => d.FullPath)
                .Returns("~/bundle");

            return new BundleDescriptorReader(file.Object);
        }

        void FilesExist(params string[] filenames)
        {
            foreach (var filename in filenames)
            {
                File.WriteAllText(Path.Combine(tempDirectory, filename), "");
            }
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
            var descriptor = reader.Read();
            descriptor.AssetFilenames.SequenceEqual(new[] { "~/bundle/test1.js", "~/bundle/test2.js" }).ShouldBeTrue();
        }

        [Fact]
        public void CommentAfterFilenameIgnored()
        {
            FilesExist("test.js");
            var reader = GetReader("test.js # comment");
            var result = reader.Read();
            result.AssetFilenames.SequenceEqual(new[] { "~/bundle/test.js" }).ShouldBeTrue();
        }

        [Fact]
        public void GivenAssetsSectionReadReturnsAssetFilenames()
        {
            FilesExist("test.js");
            var reader = GetReader("[assets]\ntest.js");
            var result = reader.Read();
            result.AssetFilenames.SequenceEqual(new[] { "~/bundle/test.js" }).ShouldBeTrue();
        }

        [Fact]
        public void GivenAssetsSectionWithTrailingCommentReadReturnsAssetFilenames()
        {
            FilesExist("test.js");
            var reader = GetReader("[assets]#comment\ntest.js");
            var result = reader.Read();
            result.AssetFilenames.SequenceEqual(new[] { "~/bundle/test.js" }).ShouldBeTrue();
        }

        [Fact]
        public void GivenReferencesSectionReadReturnsReferences()
        {
            var reader = GetReader("[references]\n../lib/other.js");
            var result = reader.Read();
            result.References.SequenceEqual(new[]{"~/lib/other.js"}).ShouldBeTrue();
        }

        [Fact]
        public void GivenBothAssetsAndReferencesSectionsReadReturnsBoth()
        {
            FilesExist("test.js");
            var reader = GetReader("[assets]\ntest.js\n[references]\n../lib/other.js");
            var result = reader.Read();
            result.AssetFilenames.SequenceEqual(new[] { "~/bundle/test.js" }).ShouldBeTrue();
            result.References.SequenceEqual(new[] { "~/lib/other.js" }).ShouldBeTrue();
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

        [Fact]
        public void GivenExternalSectionContainsNonKeyValuePair_WhenRead_ThenThrowException()
        {
            var reader = GetReader("[external]\nfail");
            Assert.Throws<Exception>(delegate
            {
                reader.Read();
            });
        }

        [Fact]
        public void GivenUnexpectedPropertyInExternalSection_WhenRead_ThenExceptionThrown()
        {
            var reader = GetReader("[external]\nunknown=fail");
            var exception = Assert.Throws<Exception>(delegate
            {
                reader.Read();
            });
            exception.Message.ShouldContain("unknown=fail");
        }

        [Fact]
        public void GivenUnexpectedSection_WhenRead_ThenExceptionThrown()
        {
            var reader = GetReader("[fail]");
            var exception = Assert.Throws<Exception>(delegate
            {
                reader.Read();
            });
            exception.Message.ShouldContain("fail");
        }

        [Fact]
        public void WhenRead_ThenReturnedDescriptorIsFromFileEqualsTrue()
        {
            var reader = GetReader("*");
            var descriptor = reader.Read();
            descriptor.IsFromFile.ShouldBeTrue();
        }

        public void Dispose()
        {
            tempDirectory.Dispose();
        }
    }
}