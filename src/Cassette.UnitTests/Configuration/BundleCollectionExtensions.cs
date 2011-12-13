using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Cassette.IO;
using Cassette.Scripts;
using Cassette.Stylesheets;
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
            var fileSource = new Mock<IFileSearch>();
            fileSource.Setup(s => s.FindFiles(It.IsAny<IDirectory>()))
                       .Returns(new[] { StubFile() })
                       .Verifiable();

            bundles.Add<TestableBundle>("~/test", fileSource.Object);

            fileSource.Verify();
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
            var fileSource = new Mock<IFileSearch>();
            var file = StubFile();
            fileSource.Setup(s => s.FindFiles(It.IsAny<IDirectory>()))
                       .Returns(new[] { file })
                       .Verifiable();
            CreateDirectory("bundle");

            bundles.AddPerSubDirectory<TestableBundle>("~", fileSource.Object);

            fileSource.Verify();
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

    public class BundleCollection_AddUrl_Tests
    {
        readonly BundleCollection bundles;
        readonly CassetteSettings settings;
        readonly Mock<IDirectory> sourceDirectory;

        public BundleCollection_AddUrl_Tests()
        {
            sourceDirectory = new Mock<IDirectory>();
            settings = new CassetteSettings("")
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
        public void WhenAddUrlWithUrlEndingWithJS_ThenScriptBundleAdded()
        {
            bundles.AddUrl("http://test.com/test.js");
            bundles["http://test.com/test.js"].ShouldBeType<ExternalScriptBundle>();
        }


        [Fact]
        public void WhenAddUrlWithUrlEndingWithUpperCaseJS_ThenScriptBundleAdded()
        {
            bundles.AddUrl("http://test.com/test.JS");
            bundles["http://test.com/test.JS"].ShouldBeType<ExternalScriptBundle>();
        }

        [Fact]
        public void WhenAddUrlWithUrlEndingWithCSS_ThenStylesheetBundleAdded()
        {
            bundles.AddUrl("http://test.com/test.css");
            bundles["http://test.com/test.css"].ShouldBeType<ExternalStylesheetBundle>();
        }

        [Fact]
        public void WhenAddUrlWithUnknownFileExtension_ThenArgumentExceptionThrown()
        {
            Assert.Throws<ArgumentException>(
                () => bundles.AddUrl("http://test.com/test")
            );
        }

        [Fact]
        public void WhenAddUrlWithAlias_ThenPathIsAlias()
        {
            bundles.AddUrlWithAlias<ScriptBundle>("http://cdn.com/jquery.js", "jquery");

            bundles.Get<ExternalScriptBundle>("jquery").Url.ShouldEqual("http://cdn.com/jquery.js");
        }

        [Fact]
        public void WhenAddUrlWithAliasAndCustomizeDelegate_ThenCustomizeAppliedToBundle()
        {
            bundles.AddUrlWithAlias<ScriptBundle>("http://cdn.com/jquery.js", "jquery", b => b.PageLocation = "test");

            bundles["jquery"].PageLocation.ShouldEqual("test");
        }

        [Fact]
        public void WhenAddUrlWithLocalAssets_ThenBundleHasAsset()
        {
            var fileSource = new Mock<IFileSearch>();
            var directory = new Mock<IDirectory>();
            var file = new Mock<IFile>();

            file.SetupGet(f => f.FullPath).Returns("~/path/file.js");
            sourceDirectory.Setup(d => d.DirectoryExists("path")).Returns(true);
            sourceDirectory.Setup(d => d.GetDirectory("path")).Returns(directory.Object);
            settings.SourceDirectory = sourceDirectory.Object;
            settings.DefaultFileSearches[typeof(ScriptBundle)] = fileSource.Object;
            fileSource.Setup(s => s.FindFiles(directory.Object)).Returns(new[] { file.Object });
            directory.Setup(d => d.GetFile("bundle.txt")).Returns(new NonExistentFile(""));
            directory.Setup(d => d.GetFile("module.txt")).Returns(new NonExistentFile(""));

            bundles.AddUrlWithLocalAssets<ScriptBundle>("http://cdn.com/jquery.js", new LocalAssetSettings { Path = "path" });

            bundles["path"].Assets[0].SourceFile.ShouldBeSameAs(file.Object);
        }

        [Fact]
        public void WhenAddUrlWithLocalAssetsUntyped_ThenBundleTypeInferedFromExtension()
        {
            var fileSource = new Mock<IFileSearch>();
            var directory = new Mock<IDirectory>();
            var file = new Mock<IFile>();

            file.SetupGet(f => f.FullPath).Returns("~/path/file.js");
            sourceDirectory.Setup(d => d.DirectoryExists("path")).Returns(true);
            sourceDirectory.Setup(d => d.GetDirectory("path")).Returns(directory.Object);
            settings.SourceDirectory = sourceDirectory.Object;
            settings.DefaultFileSearches[typeof(ScriptBundle)] = fileSource.Object;
            fileSource.Setup(s => s.FindFiles(directory.Object)).Returns(new[] { file.Object });
            directory.Setup(d => d.GetFile("bundle.txt")).Returns(new NonExistentFile(""));
            directory.Setup(d => d.GetFile("module.txt")).Returns(new NonExistentFile(""));

            bundles.AddUrlWithLocalAssets("http://cdn.com/jquery.js", new LocalAssetSettings { Path = "path" });

            bundles["path"].ShouldBeType<ExternalScriptBundle>();
        }

        [Fact]
        public void WhenAddUrlWithLocalAssets_ThenBundleCanBeAccessedByUrl()
        {
            var fileSource = new Mock<IFileSearch>();
            var directory = new Mock<IDirectory>();
            var file = new Mock<IFile>();

            file.SetupGet(f => f.FullPath).Returns("~/path/file.js");
            sourceDirectory.Setup(d => d.DirectoryExists("path")).Returns(true);
            sourceDirectory.Setup(d => d.GetDirectory("path")).Returns(directory.Object);
            settings.SourceDirectory = sourceDirectory.Object;
            settings.DefaultFileSearches[typeof(ScriptBundle)] = fileSource.Object;
            fileSource.Setup(s => s.FindFiles(directory.Object)).Returns(new[] { file.Object });
            directory.Setup(d => d.GetFile("bundle.txt")).Returns(new NonExistentFile(""));
            directory.Setup(d => d.GetFile("module.txt")).Returns(new NonExistentFile(""));

            bundles.AddUrlWithLocalAssets("http://cdn.com/jquery.js", new LocalAssetSettings { Path = "path" });

            bundles["http://cdn.com/jquery.js"].ShouldBeType<ExternalScriptBundle>();
        }

        [Fact]
        public void WhenAddUrlWithLocalAssetsWithFileSearch_ThenFileSourceUsed()
        {
            var fileSearch = new Mock<IFileSearch>();
            var directory = new Mock<IDirectory>();
            var file = new Mock<IFile>();

            file.SetupGet(f => f.FullPath).Returns("~/path/file.js");
            sourceDirectory.Setup(d => d.DirectoryExists("path")).Returns(true);
            sourceDirectory.Setup(d => d.GetDirectory("path")).Returns(directory.Object);
            settings.SourceDirectory = sourceDirectory.Object;
            fileSearch.Setup(s => s.FindFiles(directory.Object)).Returns(new[] { file.Object });
            directory.Setup(d => d.GetFile("bundle.txt")).Returns(new NonExistentFile(""));
            directory.Setup(d => d.GetFile("module.txt")).Returns(new NonExistentFile(""));

            bundles.AddUrlWithLocalAssets<ScriptBundle>("http://cdn.com/jquery.js", new LocalAssetSettings
            {
                Path = "path", 
                FileSearch = fileSearch.Object
            });

            bundles["path"].Assets[0].SourceFile.ShouldBeSameAs(file.Object);
        }

        [Fact]
        public void WhenAddUrlWithLocalAssetsSingleFile_ThenBundleHasSingleAsset()
        {
            var file = new Mock<IFile>();

            file.SetupGet(f => f.Exists).Returns(true);
            file.SetupGet(f => f.FullPath).Returns("~/jquery.js");
            sourceDirectory.Setup(d => d.GetFile("~/jquery.js"))
                           .Returns(file.Object);

            bundles.AddUrlWithLocalAssets<ScriptBundle>("http://cdn.com/jquery.js", new LocalAssetSettings
            {
                Path = "~/jquery.js"
            });

            var bundle = bundles["jquery.js"].ShouldBeType<ExternalScriptBundle>();
            bundle.Assets[0].SourceFile.ShouldBeSameAs(file.Object);
        }

        [Fact]
        public void WhenAddUrlWithFallback_ThenExternalBundleCreatedWithFallbackCondition()
        {
            var fileSource = new Mock<IFileSearch>();
            var directory = new Mock<IDirectory>();
            var file = new Mock<IFile>();

            file.SetupGet(f => f.FullPath).Returns("~/path/file.js");
            sourceDirectory.Setup(d => d.DirectoryExists("path")).Returns(true);
            sourceDirectory.Setup(d => d.GetDirectory("path")).Returns(directory.Object);
            settings.SourceDirectory = sourceDirectory.Object;
            settings.DefaultFileSearches[typeof(ScriptBundle)] = fileSource.Object;
            fileSource.Setup(s => s.FindFiles(directory.Object)).Returns(new[] { file.Object });
            directory.Setup(d => d.GetFile("bundle.txt")).Returns(new NonExistentFile(""));
            directory.Setup(d => d.GetFile("module.txt")).Returns(new NonExistentFile(""));

            bundles.AddUrlWithLocalAssets<ScriptBundle>("http://cdn.com/jquery.js", new LocalAssetSettings
            {
                Path = "path",
                FallbackCondition = "condition"
            });

            bundles.Get<ExternalScriptBundle>("path").FallbackCondition.ShouldEqual("condition");
        }
    }

    public class BundleCollection_AddPerIndividualFile_Tests
    {
        readonly BundleCollection bundles;
        readonly CassetteSettings settings;
        readonly Mock<IDirectory> sourceDirectory;
        readonly Mock<IFileSearch> fileSearch;

        public BundleCollection_AddPerIndividualFile_Tests()
        {
            var factory = new Mock<IBundleFactory<TestableBundle>>();
            factory
                .Setup(f => f.CreateBundle(It.IsAny<string>(), It.IsAny<IEnumerable<IFile>>(), It.IsAny<BundleDescriptor>()))
                .Returns<string, IEnumerable<IFile>, BundleDescriptor>((path, _, __) => new TestableBundle(path));

            sourceDirectory = new Mock<IDirectory>();
            fileSearch = new Mock<IFileSearch>();
            settings = new CassetteSettings("")
            {
                SourceDirectory = sourceDirectory.Object,
                BundleFactories = { { typeof(TestableBundle), factory.Object } },
                DefaultFileSearches = { { typeof(TestableBundle), fileSearch.Object } }
            };
            bundles = new BundleCollection(settings);
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

    public class BundleCollection_AddExplicitFiles_Tests
    {
        [Fact]
        public void AddWithExplicitFileCreatesBundleWithAsset()
        {
            using (var temp = new TempDirectory())
            {
                File.WriteAllText(Path.Combine(temp, "file1.js"), "");

                var settings = new CassetteSettings();
                var bundles = new BundleCollection(settings);
                settings.SourceDirectory = new FileSystemDirectory(temp);

                bundles.Add<ScriptBundle>("~/path", new[] { "~/file1.js" });

                bundles["~/path"].Assets[0].SourceFile.FullPath.ShouldEqual("~/file1.js");
            }
        }

        [Fact]
        public void AddWithTwoExplicitFileCreatesBundleWithTwoAssets()
        {
            using (var temp = new TempDirectory())
            {
                File.WriteAllText(Path.Combine(temp, "file1.js"), "");
                File.WriteAllText(Path.Combine(temp, "file2.js"), "");

                var settings = new CassetteSettings();
                var bundles = new BundleCollection(settings);
                settings.SourceDirectory = new FileSystemDirectory(temp);

                bundles.Add<ScriptBundle>("~/path", new[] { "~/file1.js", "~/file2.js" });

                bundles["~/path"].Assets[0].SourceFile.FullPath.ShouldEqual("~/file1.js");
                bundles["~/path"].Assets[1].SourceFile.FullPath.ShouldEqual("~/file2.js");
            }
        }

        [Fact]
        public void AddWithExplicitFileCreatesBundleThatIsSorted()
        {
            using (var temp = new TempDirectory())
            {
                File.WriteAllText(Path.Combine(temp, "file1.js"), "");

                var settings = new CassetteSettings();
                var bundles = new BundleCollection(settings);
                settings.SourceDirectory = new FileSystemDirectory(temp);

                bundles.Add<ScriptBundle>("~/path", new[] { "~/file1.js" });

                bundles["~/path"].IsSorted.ShouldBeTrue();
            }
        }

        [Fact]
        public void WhenAddWithExplicitFileNotStartingWithTilde_ThenAssetFileIsApplicationRelative()
        {
            using (var temp = new TempDirectory())
            {
                File.WriteAllText(Path.Combine(temp, "file1.js"), "");

                var settings = new CassetteSettings();
                var bundles = new BundleCollection(settings);
                settings.SourceDirectory = new FileSystemDirectory(temp);

                bundles.Add<ScriptBundle>("~/path", new[] { "file1.js" });

                bundles["~/path"].Assets[0].SourceFile.FullPath.ShouldEqual("~/file1.js");
            }
        }

        [Fact]
        public void WhenAddWithExplicitFileNotStartingWithTildeButBundleDirectoryExists_ThenAssetFileIsBundleRelative()
        {
            using (var temp = new TempDirectory())
            {
                Directory.CreateDirectory(Path.Combine(temp, "bundle"));
                File.WriteAllText(Path.Combine(temp, "bundle", "file1.js"), "");

                var settings = new CassetteSettings();
                var bundles = new BundleCollection(settings);
                settings.SourceDirectory = new FileSystemDirectory(temp);

                bundles.Add<ScriptBundle>("~/bundle", new[] { "file1.js" });

                bundles["~/bundle"].Assets[0].SourceFile.FullPath.ShouldEqual("~/bundle/file1.js");
            }
        }

        [Fact]
        public void WhenWithExplicitFileAndCustomizeAction_ThenCreatedBundleIsCustomized()
        {
            using (var temp = new TempDirectory())
            {
                File.WriteAllText(Path.Combine(temp, "file1.js"), "");

                var settings = new CassetteSettings();
                var bundles = new BundleCollection(settings);
                settings.SourceDirectory = new FileSystemDirectory(temp);

                bundles.Add<ScriptBundle>("~/path", new[] { "~/file1.js" }, b => b.PageLocation = "test");

                bundles["~/path"].PageLocation.ShouldEqual("test");
            }
        }
    }
}
