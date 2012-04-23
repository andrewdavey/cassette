using System;
using System.IO;
using System.Threading;
using Cassette.IO;
using Moq;
using Xunit;

namespace Cassette
{
    public class FileSystemWatchingBundleRebuilder_Tests : IDisposable
    {
        readonly BundleCollection bundles;
        readonly FileSystemWatchingBundleRebuilder rebuilder;
        readonly Mock<IConfiguration<BundleCollection>> bundleConfiguration;
        readonly TempDirectory tempDirectory;
        readonly Mock<IFileSearch> fileSearch;

        public FileSystemWatchingBundleRebuilder_Tests()
        {
            tempDirectory = new TempDirectory();
            var settings = new CassetteSettings
            {
                SourceDirectory = new FileSystemDirectory(tempDirectory)
            };
            bundles = new BundleCollection(settings, Mock.Of<IFileSearchProvider>(), Mock.Of<IBundleFactoryProvider>());
            bundleConfiguration = new Mock<IConfiguration<BundleCollection>>();

            var bundle = new TestableBundle("~");
            var asset1 = new StubAsset("~/test.js");
            var asset2 = new StubAsset("~/sub/test2.js");
            asset1.AddRawFileReference("~/image.png");
            bundle.Assets.Add(asset1);
            bundle.Assets.Add(asset2);
            bundles.Add(bundle);

            fileSearch = new Mock<IFileSearch>();
            fileSearch
                .Setup(s => s.IsMatch(It.IsAny<string>()))
                .Returns<string>(path => path.EndsWith(".js"));
            
            var initializer = new BundleCollectionInitializer(new[] { bundleConfiguration.Object }, new ExternalBundleGenerator(Mock.Of<IBundleFactoryProvider>(), settings));
            rebuilder = new FileSystemWatchingBundleRebuilder(settings, bundles, initializer, new[] { fileSearch.Object });
        }

        [Fact]
        public void WhenNewFileCreated_ThenRebuild()
        {
            rebuilder.Start();

            CreateFile("test.js");

            AssertBundleCollectionRebuilt();
        }

        [Fact]
        public void WhenFileCreatedNotMatchingFileSearch_ThenDontRebuild()
        {
            fileSearch.Setup(s => s.IsMatch(It.IsAny<string>())).Returns(false);            
            rebuilder.Start();

            CreateFile("test.txt");

            AssertBundleCollectionNotRebuilt();
        }

        [Fact]
        public void WhenFileDeleted_ThenRebuild()
        {
            CreateFile("test.js");
            rebuilder.Start();

            DeleteFile("test.js");

            AssertBundleCollectionRebuilt();
        }

        [Fact]
        public void WhenFileUnknownToCassetteDeleted_ThenDontRebuild()
        {
            CreateFile("unknown.js");
            rebuilder.Start();

            DeleteFile("unknown.js");

            AssertBundleCollectionNotRebuilt();
        }

        [Fact]
        public void WhenFileChanged_ThenRebuild()
        {
            CreateFile("test.js");
            rebuilder.Start();

            ChangeFile("test.js");

            AssertBundleCollectionRebuilt();
        }

        [Fact]
        public void WhenFileNotKnownByCassetteChanged_ThenDontRebuild()
        {
            CreateFile("unknown.js");
            rebuilder.Start();

            ChangeFile("unknown.js");

            AssertBundleCollectionNotRebuilt();
        }

        [Fact]
        public void WhenKnownFileRenamed_ThenRebuild()
        {
            CreateFile("test.js");
            rebuilder.Start();

            RenameFile("test.js", "test.xxx");

            AssertBundleCollectionRebuilt();
        }

        [Fact]
        public void WhenUnknownFileRenamedSoItMatchesFileSearch_ThenRebuild()
        {
            CreateFile("test.xxx");
            rebuilder.Start();

            RenameFile("test.xxx", "test.js");

            AssertBundleCollectionRebuilt();
        }

        [Fact]
        public void WhenSubDirectoryOfBundleRenamed_ThenBuild()
        {
            CreateDirectory("sub");
            CreateFile("sub/test2.js");
            rebuilder.Start();

            RenameDirectory("sub", "renamed-sub");

            AssertBundleCollectionRebuilt();
        }

        [Fact]
        public void WhenRawFileReferencedFileIsChanged_ThenRebuild()
        {
            CreateFile("image.png");
            rebuilder.Start();

            ChangeFile("image.png");

            AssertBundleCollectionRebuilt();
        }

        [Fact]
        public void WhenRawFileReferencedFileIsDeleted_ThenRebuild()
        {
            CreateFile("image.png");
            rebuilder.Start();

            DeleteFile("image.png");

            AssertBundleCollectionRebuilt();
        }

        void CreateFile(string filename)
        {
            File.WriteAllText(Path.Combine(tempDirectory, filename), "");
        }

        void CreateDirectory(string path)
        {
            Directory.CreateDirectory(Path.Combine(tempDirectory, path));
        }

        void RenameFile(string currentFilename, string newFilename)
        {
            File.Move(
                Path.Combine(tempDirectory, currentFilename),
                Path.Combine(tempDirectory, newFilename)
            );
        }

        void RenameDirectory(string currentPath, string newPath)
        {
            Directory.Move(
                Path.Combine(tempDirectory, currentPath),
                Path.Combine(tempDirectory, newPath)
            );
        }

        void ChangeFile(string filename)
        {
            File.WriteAllText(Path.Combine(tempDirectory, filename), Guid.NewGuid().ToString());
        }

        void DeleteFile(string filename)
        {
            File.Delete(Path.Combine(tempDirectory, filename));
        }

        void PauseForEvent()
        {
            Thread.Sleep(200); // Wait for the file system change event to fire.
        }

        void AssertBundleCollectionRebuilt()
        {
            PauseForEvent();
            bundleConfiguration.Verify(d => d.Configure(bundles), Times.Once());
        }

        void AssertBundleCollectionNotRebuilt()
        {
            PauseForEvent();
            bundleConfiguration.Verify(d => d.Configure(bundles), Times.Never());
        }

        public void Dispose()
        {
            rebuilder.Dispose();
            tempDirectory.Dispose();
        }
    }
}