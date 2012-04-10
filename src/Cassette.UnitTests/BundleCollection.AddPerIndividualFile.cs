using System;
using System.Collections.Generic;
using System.Linq;
using Cassette.Configuration;
using Cassette.IO;
using Moq;
using Should;
using Xunit;

namespace Cassette
{
    public class BundleCollection_AddPerIndividualFile_Tests
    {
        readonly BundleCollection bundles;
        readonly CassetteSettings settings;
        readonly Mock<IDirectory> sourceDirectory;
        readonly Mock<IFileSearch> fileSearch;

        public BundleCollection_AddPerIndividualFile_Tests()
        {
            var factory = new Mock<IBundleFactory<TestableBundle>>();
            fileSearch = new Mock<IFileSearch>();
            factory
                .Setup(f => f.CreateBundle(It.IsAny<string>(), It.IsAny<IEnumerable<IFile>>(), It.IsAny<BundleDescriptor>()))
                .Returns<string, IEnumerable<IFile>, BundleDescriptor>((path, _, __) => new TestableBundle(path));

            sourceDirectory = new Mock<IDirectory>();
            settings = new CassetteSettings()
            {
                SourceDirectory = sourceDirectory.Object,
            };

            var provider = new Mock<IBundleFactoryProvider>();
            provider.Setup(p => p.GetBundleFactory<TestableBundle>()).Returns(factory.Object);

            bundles = new BundleCollection(settings, Mock.Of<IFileSearchProvider>(), provider.Object);
        }

        [Fact]
        public void GivenTwoFiles_WhenAddPerIndividualFile_ThenTwoBundlesAreAdded()
        {
            sourceDirectory
                .Setup(d => d.GetDirectory("~"))
                .Returns(sourceDirectory.Object);
            FilesExist(sourceDirectory.Object, "~/file-a.js", "~/sub/file-b.js");
            
            bundles.AddPerIndividualFile<TestableBundle>();

            bundles.Count().ShouldEqual(2);
            bundles["~/file-a.js"].ShouldBeType<TestableBundle>();
            bundles["~/sub/file-b.js"].ShouldBeType<TestableBundle>();
        }

        [Fact]
        public void GivenFilesInSubDirectory_WhenAddPerIndividualFileOfDirectoryPath_ThenBundlesAreOnlyAddedForSubDirFiles()
        {
            var subDirectory = new Mock<IDirectory>();
            sourceDirectory
                .Setup(d => d.GetDirectory("sub"))
                .Returns(subDirectory.Object);
            FilesExist(subDirectory.Object, "~/sub/file-b.js", "~/sub/file-c.js");

            bundles.AddPerIndividualFile<TestableBundle>("sub");

            bundles.Count().ShouldEqual(2);
            bundles["~/sub/file-b.js"].ShouldBeType<TestableBundle>();
            bundles["~/sub/file-c.js"].ShouldBeType<TestableBundle>();
        }

        [Fact]
        public void GivenCustomFileSearch_WhenAddPerIndividualFile_ThenCustomFileSearchIsUsedToFindFiles()
        {
            sourceDirectory
                .Setup(d => d.GetDirectory("~"))
                .Returns(sourceDirectory.Object);
            FilesExist(sourceDirectory.Object, "~/file-a.js", "~/sub/file-b.js");

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
            sourceDirectory
                .Setup(d => d.GetDirectory("~"))
                .Returns(sourceDirectory.Object);
            FilesExist(sourceDirectory.Object, "~/file-a.js", "~/file-b.js");

            var customizeCalled = 0;
            Action<TestableBundle> customize = b => customizeCalled++;

            bundles.AddPerIndividualFile("~", customizeBundle: customize);

            customizeCalled.ShouldEqual(2);
        }

        void FilesExist(IDirectory directory, params string[] paths)
        {
            fileSearch
                .Setup(d => d.FindFiles(directory))
                .Returns(paths.Select(StubFile));
        }

        IFile StubFile(string path)
        {
            var file = new Mock<IFile>();
            file.SetupGet(f => f.FullPath).Returns(path);
            return file.Object;
        }
    }
}