using System;
using System.Collections.Generic;
using System.Linq;
using Cassette.IO;
using Cassette.Scripts;
using Cassette.Stylesheets;
using Moq;
using Should;
using Xunit;

namespace Cassette.Configuration
{
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
            Action<Bundle> customizeBundle = b => called = true;

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

            bundles.Get<ExternalScriptBundle>("jquery").Path.ShouldEqual("~/jquery");
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
            var fileSearch = new Mock<IFileSearch>();
            var directory = new Mock<IDirectory>();
            var file = new Mock<IFile>();

            file.SetupGet(f => f.FullPath).Returns("~/path/file.js");
            sourceDirectory.Setup(d => d.DirectoryExists("path")).Returns(true);
            sourceDirectory.Setup(d => d.GetDirectory("path")).Returns(directory.Object);
            settings.SourceDirectory = sourceDirectory.Object;
            settings.DefaultFileSearches[typeof(ScriptBundle)] = fileSearch.Object;
            fileSearch.Setup(s => s.FindFiles(directory.Object)).Returns(new[] { file.Object });
            BundleDescriptorDoesNotExist(directory);

            bundles.AddUrlWithLocalAssets<ScriptBundle>("http://cdn.com/jquery.js", new LocalAssetSettings { Path = "path" });

            bundles["path"].Assets[0].Path.ShouldEqual("~/path/file.js");
        }

        [Fact]
        public void WhenAddUrlWithLocalAssetsUntyped_ThenBundleTypeInferedFromExtension()
        {
            var fileSearch = new Mock<IFileSearch>();
            var directory = new Mock<IDirectory>();
            var file = new Mock<IFile>();

            file.SetupGet(f => f.FullPath).Returns("~/path/file.js");
            sourceDirectory.Setup(d => d.DirectoryExists("path")).Returns(true);
            sourceDirectory.Setup(d => d.GetDirectory("path")).Returns(directory.Object);
            settings.SourceDirectory = sourceDirectory.Object;
            settings.DefaultFileSearches[typeof(ScriptBundle)] = fileSearch.Object;
            fileSearch.Setup(s => s.FindFiles(directory.Object)).Returns(new[] { file.Object });
            BundleDescriptorDoesNotExist(directory);

            bundles.AddUrlWithLocalAssets("http://cdn.com/jquery.js", new LocalAssetSettings { Path = "path" });

            bundles["path"].ShouldBeType<ExternalScriptBundle>();
        }

        [Fact]
        public void WhenAddUrlWithLocalAssets_ThenBundleCanBeAccessedByUrl()
        {
            var fileSearch = new Mock<IFileSearch>();
            var directory = new Mock<IDirectory>();
            var file = new Mock<IFile>();

            file.SetupGet(f => f.FullPath).Returns("~/path/file.js");
            sourceDirectory.Setup(d => d.DirectoryExists("path")).Returns(true);
            sourceDirectory.Setup(d => d.GetDirectory("path")).Returns(directory.Object);
            settings.SourceDirectory = sourceDirectory.Object;
            settings.DefaultFileSearches[typeof(ScriptBundle)] = fileSearch.Object;
            fileSearch.Setup(s => s.FindFiles(directory.Object)).Returns(new[] { file.Object });
            BundleDescriptorDoesNotExist(directory);

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
            BundleDescriptorDoesNotExist(directory);

            bundles.AddUrlWithLocalAssets<ScriptBundle>("http://cdn.com/jquery.js", new LocalAssetSettings
            {
                Path = "path", 
                FileSearch = fileSearch.Object
            });

            bundles["path"].Assets[0].Path.ShouldEqual("~/path/file.js");
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
            bundle.Assets[0].Path.ShouldEqual("~/jquery.js");
        }

        [Fact]
        public void WhenAddUrlWithFallback_ThenExternalBundleCreatedWithFallbackCondition()
        {
            var fileSearch = new Mock<IFileSearch>();
            var directory = new Mock<IDirectory>();
            var file = new Mock<IFile>();

            file.SetupGet(f => f.FullPath).Returns("~/path/file.js");
            sourceDirectory.Setup(d => d.DirectoryExists("path")).Returns(true);
            sourceDirectory.Setup(d => d.GetDirectory("path")).Returns(directory.Object);
            settings.SourceDirectory = sourceDirectory.Object;
            settings.DefaultFileSearches[typeof(ScriptBundle)] = fileSearch.Object;
            fileSearch.Setup(s => s.FindFiles(directory.Object)).Returns(new[] { file.Object });
            BundleDescriptorDoesNotExist(directory);

            bundles.AddUrlWithLocalAssets<ScriptBundle>("http://cdn.com/jquery.js", new LocalAssetSettings
            {
                Path = "path",
                FallbackCondition = "condition"
            });

            bundles.Get<ExternalScriptBundle>("path").FallbackCondition.ShouldEqual("condition");
        }

        [Fact]
        public void WhenAddUrlWithLocalAssetsWherePathIsExistingBundle_ThenReplaceTheExistingBundle()
        {
            var existingBundle = new ScriptBundle("~/path");
            bundles.Add(existingBundle);

            var directory = new Mock<IDirectory>();

            sourceDirectory.Setup(d => d.DirectoryExists("path")).Returns(true);
            sourceDirectory.Setup(d => d.GetDirectory("path")).Returns(directory.Object);
            settings.SourceDirectory = sourceDirectory.Object;
            BundleDescriptorDoesNotExist(directory);

            bundles.AddUrlWithLocalAssets("http://cdn.com/jquery.js", new LocalAssetSettings
            {
                Path = "path"
            });

            bundles.Count().ShouldEqual(1);
            bundles.First().ShouldNotBeSameAs(existingBundle);
        }

        void BundleDescriptorDoesNotExist(Mock<IDirectory> directory)
        {
            directory.Setup(d => d.GetFile("bundle.txt")).Returns(new NonExistentFile(""));
            directory.Setup(d => d.GetFile("module.txt")).Returns(new NonExistentFile(""));
        }
    }
}