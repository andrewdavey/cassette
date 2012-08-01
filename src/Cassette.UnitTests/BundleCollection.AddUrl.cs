using System;
using System.Collections.Generic;
using System.Linq;
using Cassette.BundleProcessing;
using Cassette.IO;
using Cassette.Scripts;
using Cassette.Stylesheets;
using Moq;
using Should;
using Xunit;

namespace Cassette
{
    public class BundleCollection_AddUrl_Tests : BundleCollectionTestsBase
    {
        public BundleCollection_AddUrl_Tests()
        {
            bundleFactoryProvider
                .Setup(f => f.GetBundleFactory<ScriptBundle>())
                .Returns(new ScriptBundleFactory(() => Mock.Of<IBundlePipeline<ScriptBundle>>()));

            bundleFactoryProvider
                .Setup(f => f.GetBundleFactory<StylesheetBundle>())
                .Returns(new StylesheetBundleFactory(() => Mock.Of<IBundlePipeline<StylesheetBundle>>()));
        }

        [Fact]
        public void WhenAddUrlOfScriptBundle_ThenExternalScriptBundleAdded()
        {
            var url = "http://cdn.com/jquery.js";
            var factory = new Mock<IBundleFactory<ScriptBundle>>();
            SetBundleFactory(factory);
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
            SetBundleFactory(factory);
            factory
                .Setup(f => f.CreateBundle(
                    url,
                    It.IsAny<IEnumerable<IFile>>(),
                    It.IsAny<BundleDescriptor>()
                ))
                .Returns(new ExternalScriptBundle(url));

            var called = false;
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
            settings.SourceDirectory = new FakeFileSystem
            {
                "~/path/file.js"
            };

            bundles.AddUrlWithLocalAssets<ScriptBundle>("http://cdn.com/jquery.js", new LocalAssetSettings { Path = "path" });

            bundles["path"].Assets[0].Path.ShouldEqual("~/path/file.js");
        }

        [Fact]
        public void WhenAddUrlWithLocalAssetsUntyped_ThenBundleTypeInferedFromExtension()
        {
            settings.SourceDirectory = new FakeFileSystem
            {
                "~/path/file.js"
            };

            bundles.AddUrlWithLocalAssets("http://cdn.com/jquery.js", new LocalAssetSettings { Path = "path" });

            bundles["path"].ShouldBeType<ExternalScriptBundle>();
        }

        [Fact]
        public void WhenAddUrlWithLocalAssets_ThenBundleCanBeAccessedByUrl()
        {
            settings.SourceDirectory = new FakeFileSystem
            {
                "~/path/file.js"
            };

            bundles.AddUrlWithLocalAssets("http://cdn.com/jquery.js", new LocalAssetSettings { Path = "path" });

            bundles["http://cdn.com/jquery.js"].ShouldBeType<ExternalScriptBundle>();
        }

        [Fact]
        public void WhenAddUrlWithLocalAssetsWithFileSearch_ThenFileSourceUsed()
        {
            settings.SourceDirectory = new FakeFileSystem
            {
                "~/path/file.js"
            };

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
            settings.SourceDirectory = new FakeFileSystem
            {
                "~/jquery.js"
            };

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
            settings.SourceDirectory = new FakeFileSystem
            {
                "~/path/file.js"
            };

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

            settings.SourceDirectory = new FakeFileSystem
            {
                "~/path/file.js"
            };

            bundles.AddUrlWithLocalAssets("http://cdn.com/jquery.js", new LocalAssetSettings
            {
                Path = "path"
            });

            bundles.Count().ShouldEqual(1);
            bundles.First().ShouldNotBeSameAs(existingBundle);
        }
    }
}