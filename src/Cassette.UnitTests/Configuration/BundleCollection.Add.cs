using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Cassette.IO;
using Moq;
using Should;
using Xunit;

namespace Cassette.Configuration
{
    public class BundleCollection_Add_Tests : IDisposable
    {
        TestableBundle createdBundle;
        readonly BundleCollection bundles;
        readonly TempDirectory tempDirectory;
        readonly Mock<IBundleFactory<TestableBundle>> factory;
        readonly Mock<IFileSearch> defaultFileSource;
        readonly CassetteSettings settings;

        public BundleCollection_Add_Tests()
        {
            tempDirectory = new TempDirectory();
            CreateDirectory("test");
            factory = new Mock<IBundleFactory<TestableBundle>>();
            factory.Setup(f => f.CreateBundle(It.IsAny<string>(), It.IsAny<IEnumerable<IFile>>(), It.IsAny<BundleDescriptor>()))
                .Returns<string, IEnumerable<IFile>, BundleDescriptor>(
                    (path, files, d) => (createdBundle = new TestableBundle(path))
                );
            defaultFileSource = new Mock<IFileSearch>();
            settings = new CassetteSettings("")
            {
                SourceDirectory = new FileSystemDirectory(tempDirectory),
                BundleFactories = { { typeof(TestableBundle), factory.Object } },
                DefaultFileSearches = { { typeof(TestableBundle), defaultFileSource.Object } }
            };
            bundles = new BundleCollection(settings);
        }

        [Fact]
        public void GivenDefaultFileSourceReturnsAFile_WhenAddDirectoryPath_ThenFactoryUsedToCreateBundle()
        {
            var file = StubFile();
            defaultFileSource
                .Setup(s => s.FindFiles(It.IsAny<IDirectory>()))
                .Returns(new[] { file });

            bundles.Add<TestableBundle>("~/test");

            factory.Verify(f => f.CreateBundle(
                "~/test",
                It.Is<IEnumerable<IFile>>(files => files.SequenceEqual(new[] { file })),
                It.Is<BundleDescriptor>(d => d.AssetFilenames.Single() == "*"))
                );

            bundles["~/test"].ShouldBeSameAs(createdBundle);
        }

        [Fact]
        public void WhenAddWithDirectoryPathAndAssetSource_ThenSourceIsUsedToGetAssets()
        {
            var fileSearch = new Mock<IFileSearch>();
            fileSearch.Setup(s => s.FindFiles(It.IsAny<IDirectory>()))
                .Returns(new[] { StubFile() })
                .Verifiable();

            bundles.Add<TestableBundle>("~/test", fileSearch.Object);

            fileSearch.Verify();
        }

        [Fact]
        public void WhenAddWithCustomizeAction_ThenCustomizeActionCalledWithTheBundle()
        {
            defaultFileSource
                .Setup(s => s.FindFiles(It.IsAny<IDirectory>()))
                .Returns(new[] { StubFile() });

            Bundle bundle = null;
            Action<TestableBundle> action = b => bundle = b;

            bundles.Add("~/test", action);

            bundle.ShouldBeSameAs(createdBundle);
        }

        [Fact]
        public void GivenFilePath_WhenAdd_ThenBundleAdded()
        {
            File.WriteAllText(Path.Combine(tempDirectory, "file.js"), "");
            bundles.Add<TestableBundle>("~/file.js");

            bundles["~/file.js"].ShouldBeType<TestableBundle>();
        }

        [Fact]
        public void GivenFilePath_WhenAddFileWithoutPathTildePrefix_ThenBundleFactoryIsCalledWithBundleDescriptorHavingFullFilePathForAsset()
        {
            File.WriteAllText(Path.Combine(tempDirectory, "file.js"), "");
            bundles.Add<TestableBundle>("file.js");

            factory.Verify(f => f.CreateBundle(
                "~/file.js",
                It.IsAny<IEnumerable<IFile>>(),
                It.Is<BundleDescriptor>(
                    descriptor => descriptor.AssetFilenames.Single().Equals("~/file.js")
                    )
                                    ));
        }

        [Fact]
        public void GivenPathThatDoesNotExist_WhenAddWith_ThenThrowException()
        {
            Assert.Throws<DirectoryNotFoundException>(
                () => bundles.Add<TestableBundle>("~/does-not-exist")
                );
        }

        [Fact]
        public void GivenBundleDescriptorFile_WhenAdd_ThenDescriptorPassedToFactory()
        {
            File.WriteAllText(Path.Combine(tempDirectory, "bundle.txt"), "b.js\na.js");

            var fileA = StubFile("~/a.js");
            var fileB = StubFile("~/b.js");
            defaultFileSource
                .Setup(s => s.FindFiles(It.IsAny<IDirectory>()))
                .Returns(new[] { fileA, fileB });

            bundles.Add<TestableBundle>("~");

            factory.Verify(f => f.CreateBundle(
                "~",
                It.IsAny<IEnumerable<IFile>>(),
                It.Is<BundleDescriptor>(d => d.AssetFilenames.SequenceEqual(new[] { "~/b.js", "~/a.js" }))
            ));
        }

        void CreateDirectory(string path)
        {
            Directory.CreateDirectory(Path.Combine(tempDirectory, path));
        }

        IFile StubFile(string path = "")
        {
            var file = new Mock<IFile>();
            file.SetupGet(a => a.FullPath).Returns(path);
            return file.Object;
        }

        public void Dispose()
        {
            tempDirectory.Dispose();
        }
    }
}