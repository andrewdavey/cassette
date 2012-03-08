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
    public class BundleCollection_AddPerSubDirectory_Tests : IDisposable
    {
        readonly TempDirectory tempDirectory;
        readonly CassetteSettings settings;
        readonly BundleCollection bundles;
        TestableBundle createdBundle;
        readonly Mock<IBundleFactory<TestableBundle>> factory;
        readonly Mock<IFileSearch> defaultAssetSource;

        public BundleCollection_AddPerSubDirectory_Tests()
        {
            tempDirectory = new TempDirectory();
            factory = new Mock<IBundleFactory<TestableBundle>>();
            factory.Setup(f => f.CreateBundle(It.IsAny<string>(), It.IsAny<IEnumerable<IFile>>(), It.IsAny<BundleDescriptor>()))
                .Returns<string, IEnumerable<IFile>, BundleDescriptor>(
                    (path, files, d) => createdBundle = new TestableBundle(path)
                );
            defaultAssetSource = new Mock<IFileSearch>();
            settings = new CassetteSettings("")
            {
                SourceDirectory = new FileSystemDirectory(tempDirectory),
                BundleFactories = { { typeof(TestableBundle), factory.Object } },
                DefaultFileSearches = { { typeof(TestableBundle), defaultAssetSource.Object } }
            };
            bundles = new BundleCollection(settings);
        }

        [Fact]
        public void GivenTwoSubDirectoriesWithFiles_WhenAddPerSubDirectory_ThenTwoBundlesAreAdded()
        {
            CreateDirectory("bundle-a");
            CreateDirectory("bundle-b");

            defaultAssetSource.Setup(s => s.FindFiles(It.IsAny<IDirectory>()))
                .Returns(() => new[] { StubFile() });

            bundles.AddPerSubDirectory<TestableBundle>("~");

            bundles["~/bundle-a"].ShouldBeType<TestableBundle>();
            bundles["~/bundle-b"].ShouldBeType<TestableBundle>();
        }

        [Fact]
        public void GivenCustomAssetSource_WhenAddPerSubDirectory_ThenAssetSourceIsUsedToGetAssets()
        {
            var fileSearch = new Mock<IFileSearch>();
            var file = StubFile();
            fileSearch.Setup(s => s.FindFiles(It.IsAny<IDirectory>()))
                .Returns(new[] { file })
                .Verifiable();
            CreateDirectory("bundle");

            bundles.AddPerSubDirectory<TestableBundle>("~", fileSearch.Object);

            fileSearch.Verify();
        }

        [Fact]
        public void GivenBundleCustomizeAction_WhenAddPerSubDirectory_ThenActionIsCalledWithBundle()
        {
            defaultAssetSource.Setup(s => s.FindFiles(It.IsAny<IDirectory>()))
                .Returns(new[] { StubFile() });
            CreateDirectory("bundle");

            Bundle bundle = null;
            bundles.AddPerSubDirectory<TestableBundle>("~", b => bundle = b);

            bundle.ShouldBeSameAs(createdBundle);
        }
        
        [Fact]
        public void GivenHiddenDirectory_WhenAddPerSubDirectory_ThenDirectoryIsIgnored()
        {
            CreateDirectory("test");
            File.SetAttributes(Path.Combine(tempDirectory, "test"), FileAttributes.Directory | FileAttributes.Hidden);

            bundles.AddPerSubDirectory<TestableBundle>("~");

            bundles.ShouldBeEmpty();
        }

        [Fact]
        public void GivenEmptyDirectory_WhenAddPerSubDirectory_ThenDirectoryIsIgnored()
        {
            CreateDirectory("test");

            bundles.AddPerSubDirectory<TestableBundle>("~");

            bundles.ShouldBeEmpty();
        }

        [Fact]
        public void GivenDirectoryWithExternalBundleDescriptorButNoAssets_WhenAddPerSubDirectory_ThenBundleCreatedForDirectory()
        {
            CreateDirectory("test");
            File.WriteAllText(
                Path.Combine(tempDirectory, "test", "bundle.txt"), 
                "[external]" + Environment.NewLine + "url=http://example.org/"
                );
            bundles.AddPerSubDirectory<TestableBundle>("~");
            bundles.Count().ShouldEqual(1);
        }

        [Fact]
        public void GivenTopLevelDirectoryWithExternalBundleDescriptorButNoAssets_WhenAddPerSubDirectory_ThenBundleCreatedForDirectory()
        {
            var bundle = new Mock<TestableBundle>("~");
            bundle.As<IExternalBundle>();
            factory.Setup(f => f.CreateBundle(It.IsAny<string>(), It.IsAny<IEnumerable<IFile>>(), It.IsAny<BundleDescriptor>()))
                .Returns<string, IEnumerable<IFile>, BundleDescriptor>(
                    (path, files, d) => createdBundle = bundle.Object
                );

            File.WriteAllText(
                Path.Combine(tempDirectory, "bundle.txt"),
                "[external]" + Environment.NewLine + "url=http://example.org/"
                );
            bundles.AddPerSubDirectory<TestableBundle>("~");
            bundles.Count().ShouldEqual(1);
        }

        [Fact]
        public void GivenTopLevelDirectoryHasFilesAndSubDirectory_WhenAddPerSubDirectory_ThenBundleAlsoCreatedForTopLevel()
        {
            File.WriteAllText(Path.Combine(tempDirectory, "file-a.js"), "");
            CreateDirectory("test");
            File.WriteAllText(Path.Combine(tempDirectory, "test", "file-b.js"), "");
            defaultAssetSource
                .SetupSequence(s => s.FindFiles(It.IsAny<IDirectory>()))
                .Returns(new[] { StubFile(mock => mock.SetupGet(f => f.Directory).Returns(settings.SourceDirectory)) })
                .Returns(new[] { StubFile() });

            bundles.AddPerSubDirectory<TestableBundle>("~");

            bundles.Count().ShouldEqual(2);

            factory.Verify(f => f.CreateBundle(
                "~",
                It.Is<IEnumerable<IFile>>(files => files.Count() == 1),
                It.IsAny<BundleDescriptor>())
                );
            factory.Verify(f => f.CreateBundle(
                "~/test",
                It.Is<IEnumerable<IFile>>(files => files.Count() == 1),
                It.IsAny<BundleDescriptor>())
                );
        }

        [Fact]
        public void GivenTopLevelDirectoryHasFilesAndSubDirectory_WhenAddPerSubDirectoryWithCustomizeAction_ThenBundleForTopLevelIsCustomized()
        {
            File.WriteAllText(Path.Combine(tempDirectory, "file-a.js"), "");
            CreateDirectory("test");
            File.WriteAllText(Path.Combine(tempDirectory, "test", "file-b.js"), "");
            defaultAssetSource
                .SetupSequence(s => s.FindFiles(It.IsAny<IDirectory>()))
                .Returns(new[] { StubFile(mock => mock.SetupGet(f => f.Directory).Returns(settings.SourceDirectory)) })
                .Returns(new[] { StubFile() });

            factory.Setup(f => f.CreateBundle(
                "~",
                It.Is<IEnumerable<IFile>>(files => files.Count() == 1),
                It.IsAny<BundleDescriptor>())
                ).Returns(new TestableBundle("~"));

            bundles.AddPerSubDirectory<TestableBundle>("~", b => b.PageLocation = "test");

            bundles["~"].PageLocation.ShouldEqual("test");
        }

        [Fact]
        public void GivenTopLevelDirectoryHasFilesAndSubDirectory_WhenAddPerSubDirectoryWithExcludeTopLevelTrue_ThenBundleNotCreatedForTopLevel()
        {
            File.WriteAllText(Path.Combine(tempDirectory, "file-a.js"), "");
            CreateDirectory("test");
            File.WriteAllText(Path.Combine(tempDirectory, "test", "file-b.js"), "");
            defaultAssetSource
                .Setup(s => s.FindFiles(It.IsAny<IDirectory>()))
                .Returns(new[] { StubFile() });

            bundles.AddPerSubDirectory<TestableBundle>("~", excludeTopLevel: true);

            bundles.Count().ShouldEqual(1);
            bundles["~/test"].ShouldBeType<TestableBundle>();
        }

        void CreateDirectory(string path)
        {
            Directory.CreateDirectory(Path.Combine(tempDirectory, path));
        }

        IFile StubFile(Action<Mock<IFile>> customizeMock = null)
        {
            var file = new Mock<IFile>();
            file.SetupGet(a => a.FullPath).Returns("");
            if (customizeMock != null) customizeMock(file);
            return file.Object;
        }

        public void Dispose()
        {
            tempDirectory.Dispose();
        }
    }
}