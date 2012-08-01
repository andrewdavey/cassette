using System;
using System.Linq;
using Cassette.IO;
using Moq;
using Should;
using Xunit;

namespace Cassette
{
    public class BundleCollection_AddPerIndividualFile_Tests : BundleCollectionTestsBase
    {
        [Fact]
        public void GivenTwoFiles_WhenAddPerIndividualFile_ThenTwoBundlesAreAdded()
        {
            settings.SourceDirectory = new FakeFileSystem
            {
                "~/file-a.js",
                "~/sub/file-b.js"
            };

            bundles.AddPerIndividualFile<TestableBundle>();

            bundles.Count().ShouldEqual(2);
            bundles["~/file-a.js"].ShouldBeType<TestableBundle>();
            bundles["~/sub/file-b.js"].ShouldBeType<TestableBundle>();
        }

        [Fact]
        public void GivenFilesInSubDirectory_WhenAddPerIndividualFileOfDirectoryPath_ThenBundlesAreOnlyAddedForSubDirFiles()
        {
            settings.SourceDirectory = new FakeFileSystem
            {
                "~/sub/file-b.js",
                "~/sub/file-c.js"
            };

            bundles.AddPerIndividualFile<TestableBundle>("sub");

            bundles.Count().ShouldEqual(2);
            bundles["~/sub/file-b.js"].ShouldBeType<TestableBundle>();
            bundles["~/sub/file-c.js"].ShouldBeType<TestableBundle>();
        }

        [Fact]
        public void GivenCustomFileSearch_WhenAddPerIndividualFile_ThenCustomFileSearchIsUsedToFindFiles()
        {
            settings.SourceDirectory = new FakeFileSystem
            {
                "~/file-a.js",
                "~/sub/file-b.js"
            };

            var customFileSearch = new Mock<IFileSearch>();
            customFileSearch
                .Setup(s => s.FindFiles(It.IsAny<IDirectory>()))
                .Returns(new IFile[0])
                .Verifiable();

            bundles.AddPerIndividualFile<TestableBundle>("~", customFileSearch.Object);

            customFileSearch.Verify();
        }

        [Fact]
        public void GivenCustomizeAction_WhenAddPerIndividualFile_ThenActionCalledForEachBundle()
        {
            settings.SourceDirectory = new FakeFileSystem
            {
                "~/file-a.js",
                "~/file-b.js"
            };

            var customizeCalled = 0;
            Action<TestableBundle> customize = b => customizeCalled++;

            bundles.AddPerIndividualFile("~", customizeBundle: customize);

            customizeCalled.ShouldEqual(2);
        }
    }
}