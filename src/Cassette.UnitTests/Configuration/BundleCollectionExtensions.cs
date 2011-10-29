using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Cassette.IO;
using Cassette.Scripts;
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
        readonly Mock<IFileSource> defaultFileSource;
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
            defaultFileSource = new Mock<IFileSource>();
            settings = new CassetteSettings
            {
                SourceDirectory = new FileSystemDirectory(tempDirectory),
                BundleFactories = { { typeof(TestableBundle), factory.Object } },
                DefaultFileSources = { { typeof(TestableBundle), defaultFileSource.Object } }
            };
            bundles = new BundleCollection(settings);
        }

        [Fact]
        public void GivenDefaultFileSourceReturnsAFile_WhenAddDirectoryPath_ThenFactoryUsedToCreateBundle()
        {
            var file = StubFile();
            defaultFileSource
                .Setup(s => s.GetFiles(It.IsAny<IDirectory>()))
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
            var fileSource = new Mock<IFileSource>();
            fileSource.Setup(s => s.GetFiles(It.IsAny<IDirectory>()))
                       .Returns(new[] { StubFile() })
                       .Verifiable();

            bundles.Add<TestableBundle>("~/test", fileSource.Object);

            fileSource.Verify();
        }

        [Fact]
        public void WhenAddWithCustomizeAction_ThenCustomizeActionCalledWithTheBundle()
        {
            defaultFileSource
                .Setup(s => s.GetFiles(It.IsAny<IDirectory>()))
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
                .Setup(s => s.GetFiles(It.IsAny<IDirectory>()))
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

    public class BundleCollection_AddPerSubDirectory_Tests : IDisposable
    {
        readonly TempDirectory tempDirectory;
        readonly CassetteSettings settings;
        readonly BundleCollection bundles;
        TestableBundle createdBundle;
        readonly Mock<IBundleFactory<TestableBundle>> factory;
        readonly Mock<IFileSource> defaultAssetSource;

        public BundleCollection_AddPerSubDirectory_Tests()
        {
            tempDirectory = new TempDirectory();
            factory = new Mock<IBundleFactory<TestableBundle>>();
            factory.Setup(f => f.CreateBundle(It.IsAny<string>(), It.IsAny<IEnumerable<IFile>>(), It.IsAny<BundleDescriptor>()))
                   .Returns<string, IEnumerable<IFile>, BundleDescriptor>(
                       (path, files, d) => createdBundle = new TestableBundle(path)
                   );
            defaultAssetSource = new Mock<IFileSource>();
            settings = new CassetteSettings
            {
                SourceDirectory = new FileSystemDirectory(tempDirectory),
                BundleFactories = { { typeof(TestableBundle), factory.Object } },
                DefaultFileSources = { { typeof(TestableBundle), defaultAssetSource.Object } }
            };
            bundles = new BundleCollection(settings);
        }

        [Fact]
        public void GivenTwoSubDirectories_WhenAddPerSubDirectory_ThenTwoBundlesAreAdded()
        {
            CreateDirectory("bundle-a");
            CreateDirectory("bundle-b");

            bundles.AddPerSubDirectory<TestableBundle>("~");

            bundles["~/bundle-a"].ShouldBeType<TestableBundle>();
            bundles["~/bundle-b"].ShouldBeType<TestableBundle>();
        }

        [Fact]
        public void GivenCustomAssetSource_WhenAddPerSubDirectory_ThenAssetSourceIsUsedToGetAssets()
        {
            var fileSource = new Mock<IFileSource>();
            var file = StubFile();
            fileSource.Setup(s => s.GetFiles(It.IsAny<IDirectory>()))
                       .Returns(new[] { file })
                       .Verifiable();
            CreateDirectory("bundle");

            bundles.AddPerSubDirectory<TestableBundle>("~", fileSource.Object);

            fileSource.Verify();
        }

        [Fact]
        public void GivenBundleCustomizeAction_WhenAddPerSubDirectory_ThenActionIsCalledWithBundle()
        {
            defaultAssetSource.Setup(s => s.GetFiles(It.IsAny<IDirectory>()))
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
        public void GivenTopLevelDirectoryHasFilesAndSubDirectory_WhenAddPerSubDirectory_ThenBundleAlsoCreatedForTopLevel()
        {
            File.WriteAllText(Path.Combine(tempDirectory, "file-a.js"), "");
            CreateDirectory("test");
            File.WriteAllText(Path.Combine(tempDirectory, "test", "file-b.js"), "");
            defaultAssetSource
                .SetupSequence(s => s.GetFiles(It.IsAny<IDirectory>()))
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
        public void GivenTopLevelDirectoryHasFilesAndSubDirectory_WhenAddPerSubDirectoryWithExcludeTopLevelTrue_ThenBundleNotCreatedForTopLevel()
        {
            File.WriteAllText(Path.Combine(tempDirectory, "file-a.js"), "");
            CreateDirectory("test");
            File.WriteAllText(Path.Combine(tempDirectory, "test", "file-b.js"), "");
            defaultAssetSource
                .Setup(s => s.GetFiles(It.IsAny<IDirectory>()))
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

    public class BundleCollection_AddUrl_Tests
    {
        readonly BundleCollection bundles;
        readonly CassetteSettings settings;
        Mock<IDirectory> sourceDirectory;

        public BundleCollection_AddUrl_Tests()
        {
            sourceDirectory = new Mock<IDirectory>();
            settings = new CassetteSettings
            {
                SourceDirectory = sourceDirectory.Object
            };
            bundles = new BundleCollection(settings);
        }

        [Fact]
        public void WhenAddUrlOfScriptBundle_ThenExternalScriptBundleAdded()
        {
            var url = "http://cdn.com/jquery.js";
            var factory = new Mock<IBundleFactory<ScriptBundle>>(); 
            settings.BundleFactories[typeof(ScriptBundle)] = factory.Object;
            factory.Setup(f => f.CreateBundle(
                url,
                It.IsAny<IEnumerable<IFile>>(),
                It.IsAny<BundleDescriptor>()
            )).Returns(new ExternalScriptBundle(url));

            bundles.AddUrl<ScriptBundle>(url);

            bundles[url].ShouldBeType<ExternalScriptBundle>();
        }

        [Fact]
        public void WhenAddUrlOfScriptBundleWithCustomizeDelegate_ThenCustomizeDelegateCalled()
        {
            var url = "http://cdn.com/jquery.js";
            var factory = new Mock<IBundleFactory<ScriptBundle>>();
            settings.BundleFactories[typeof(ScriptBundle)] = factory.Object;
            factory.Setup(f => f.CreateBundle(
                url,
                It.IsAny<IEnumerable<IFile>>(),
                It.IsAny<BundleDescriptor>()
            )).Returns(new ExternalScriptBundle(url));

            bool called = false;
            Action<ScriptBundle> customizeBundle = b => called = true;

            bundles.AddUrl(url, customizeBundle);

            called.ShouldBeTrue();
        }

        [Fact]
        public void WhenAddUrlWithAlias_ThenPathIsAlias()
        {
            bundles.AddUrl("http://cdn.com/jquery.js").WithAlias("jquery");

            bundles.Get<ExternalScriptBundle>("jquery").Url.ShouldEqual("http://cdn.com/jquery.js");
        }

        [Fact]
        public void WhenAddUrlWithCustomizeDelegateAndWithAlias_ThenCustomizeAppliedToBundle()
        {
            bundles.AddUrl("http://cdn.com/jquery.js", b => b.PageLocation = "test").WithAlias("jquery");

            bundles["jquery"].PageLocation.ShouldEqual("test");
        }

        [Fact]
        public void WhenAddUrlWithDebug_ThenBundleHasAsset()
        {
            var fileSource = new Mock<IFileSource>();
            var directory = new Mock<IDirectory>();
            var file = new Mock<IFile>();

            file.SetupGet(f => f.FullPath).Returns("~/path/file.js");
            sourceDirectory.Setup(d => d.DirectoryExists("path")).Returns(true);
            sourceDirectory.Setup(d => d.GetDirectory("path")).Returns(directory.Object);
            settings.SourceDirectory = sourceDirectory.Object;
            settings.DefaultFileSources[typeof(ScriptBundle)] = fileSource.Object;
            fileSource.Setup(s => s.GetFiles(directory.Object)).Returns(new[] { file.Object });
            directory.Setup(d => d.GetFile("bundle.txt")).Returns(new NonExistentFile(""));

            bundles.AddUrl("http://cdn.com/jquery.js").WithDebug("path");

            bundles["path"].Assets[0].SourceFile.ShouldBeSameAs(file.Object);
        }

        [Fact]
        public void WhenAddUrlWithDebugWithFileSource_ThenFileSourceUsed()
        {
            var fileSource = new Mock<IFileSource>();
            var directory = new Mock<IDirectory>();
            var file = new Mock<IFile>();

            file.SetupGet(f => f.FullPath).Returns("~/path/file.js");
            sourceDirectory.Setup(d => d.DirectoryExists("path")).Returns(true);
            sourceDirectory.Setup(d => d.GetDirectory("path")).Returns(directory.Object);
            settings.SourceDirectory = sourceDirectory.Object;
            fileSource.Setup(s => s.GetFiles(directory.Object)).Returns(new[] { file.Object });
            directory.Setup(d => d.GetFile("bundle.txt")).Returns(new NonExistentFile(""));

            bundles.AddUrl("http://cdn.com/jquery.js").WithDebug("path", fileSource.Object);

            bundles["path"].Assets[0].SourceFile.ShouldBeSameAs(file.Object);
        }

        [Fact]
        public void WhenAddWithDebugSingleFile_ThenBundleHasSingleAsset()
        {
            var file = new Mock<IFile>();

            file.SetupGet(f => f.Exists).Returns(true);
            file.SetupGet(f => f.FullPath).Returns("~/jquery.js");
            sourceDirectory.Setup(d => d.GetFile("~/jquery.js"))
                           .Returns(file.Object);

            bundles.AddUrl("http://cdn.com/jquery.js").WithDebug("~/jquery.js");

            var bundle = bundles["jquery.js"].ShouldBeType<ExternalScriptBundle>();
            bundle.Assets[0].SourceFile.ShouldBeSameAs(file.Object);
        }

        [Fact]
        public void WhenAddUrlWithFallback_ThenExternalBundleCreatedWithFallbackCondition()
        {
            var fileSource = new Mock<IFileSource>();
            var directory = new Mock<IDirectory>();
            var file = new Mock<IFile>();

            file.SetupGet(f => f.FullPath).Returns("~/path/file.js");
            sourceDirectory.Setup(d => d.DirectoryExists("path")).Returns(true);
            sourceDirectory.Setup(d => d.GetDirectory("path")).Returns(directory.Object);
            settings.SourceDirectory = sourceDirectory.Object;
            settings.DefaultFileSources[typeof(ScriptBundle)] = fileSource.Object;
            fileSource.Setup(s => s.GetFiles(directory.Object)).Returns(new[] { file.Object });
            directory.Setup(d => d.GetFile("bundle.txt")).Returns(new NonExistentFile(""));

            bundles.AddUrl("http://cdn.com/jquery.js").WithFallback("condition", "path");

            bundles.Get<ExternalScriptBundle>("path").FallbackCondition.ShouldEqual("condition");
        }

        [Fact]
        public void WhenAddUrlWithFallbackWithFileSource_ThenFileSourceIsUsed()
        {
            var fileSource = new Mock<IFileSource>();
            var directory = new Mock<IDirectory>();
            var file = new Mock<IFile>();

            file.SetupGet(f => f.FullPath).Returns("~/path/file.js");
            sourceDirectory.Setup(d => d.DirectoryExists("path")).Returns(true);
            sourceDirectory.Setup(d => d.GetDirectory("path")).Returns(directory.Object);
            settings.SourceDirectory = sourceDirectory.Object;
            fileSource.Setup(s => s.GetFiles(directory.Object)).Returns(new[] { file.Object });
            directory.Setup(d => d.GetFile("bundle.txt")).Returns(new NonExistentFile(""));

            bundles.AddUrl("http://cdn.com/jquery.js").WithFallback("condition", "path", fileSource.Object);

            bundles["path"].Assets[0].SourceFile.ShouldBeSameAs(file.Object);
        }

        [Fact]
        public void WhenAddWithFallbackSingleFile_ThenBundleHasSingleAsset()
        {
            var file = new Mock<IFile>();

            file.SetupGet(f => f.Exists).Returns(true);
            file.SetupGet(f => f.FullPath).Returns("~/jquery.js");
            sourceDirectory.Setup(d => d.GetFile("~/jquery.js"))
                           .Returns(file.Object);

            bundles.AddUrl("http://cdn.com/jquery.js").WithFallback("condition", "~/jquery.js");

            var bundle = bundles["jquery.js"].ShouldBeType<ExternalScriptBundle>();
            bundle.Assets[0].SourceFile.ShouldBeSameAs(file.Object);
        }
    }
}