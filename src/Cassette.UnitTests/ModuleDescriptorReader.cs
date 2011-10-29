#region License
/*
Copyright 2011 Andrew Davey

This file is part of Cassette.

Cassette is free software: you can redistribute it and/or modify it under the 
terms of the GNU General Public License as published by the Free Software 
Foundation, either version 3 of the License, or (at your option) any later 
version.

Cassette is distributed in the hope that it will be useful, but WITHOUT ANY 
WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS 
FOR A PARTICULAR PURPOSE. See the GNU General Public License for more details.

You should have received a copy of the GNU General Public License along with 
Cassette. If not, see http://www.gnu.org/licenses/.
*/
#endregion

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
                .Setup(s => s.Open(FileMode.Open, FileAccess.Read, FileShare.ReadWrite))
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
        public void HandlesSlashesAsDirectorySeparators()
        {
            char systemPathSeparator = Path.DirectorySeparatorChar;
            string[] givenFiles = new[]
            {
                "folder" + systemPathSeparator + "test1.js"
                , "folder" + systemPathSeparator + "test2.js"
            };

            Console.WriteLine(givenFiles[0]);
            Console.WriteLine(givenFiles[1]);

            FilesExist(givenFiles);
            var reader = GetReader("folder/test1.js\nfolder/test2.js");
            var result = reader.Read();
            result.AssetFilenames.SequenceEqual(givenFiles).ShouldBeTrue();
        }

        [Fact]
        public void HandlesBackslashesAsDirectorySeparators()
        {
            char systemPathSeparator = Path.DirectorySeparatorChar;
            string[] givenFiles = new[]
            {
                "folder" + systemPathSeparator + "test1.js"
                , "folder" + systemPathSeparator + "test2.js"
            };

            FilesExist(givenFiles);
            var reader = GetReader("folder\\test1.js\nfolder\\test2.js");
            var result = reader.Read();
            result.AssetFilenames.SequenceEqual(givenFiles).ShouldBeTrue();
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
        public void DirectoryWithAsteriskIncludesAllFilesFromSubdirectory()
        {
            FilesExist("shared\\shared-test1.js", "shared\\shared-test2.js", "app\\app-test1.js", "app\\app-test2.js");
            var reader = GetReader("shared\\*\napp\\*");
            var result = reader.Read();
            result.AssetFilenames.SequenceEqual(new[] { "shared\\shared-test1.js", "shared\\shared-test2.js", "app\\app-test1.js", "app\\app-test2.js" }).ShouldBeTrue();
        }

        [Fact]
        public void DirectoryWithAsteriskIncludesAllFilesFromSubdirectoryNotAlreadyAdded()
        {
            FilesExist("shared\\shared-test1.js", "shared\\shared-test2.js", "app\\app-test1.js", "app\\app-test2.js");
            var reader = GetReader("shared\\shared-test2.js\nshared\\*\napp\\*");
            var result = reader.Read();
            result.AssetFilenames.SequenceEqual(new[] { "shared\\shared-test2.js", "shared\\shared-test1.js", "app\\app-test1.js", "app\\app-test2.js" }).ShouldBeTrue();
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
