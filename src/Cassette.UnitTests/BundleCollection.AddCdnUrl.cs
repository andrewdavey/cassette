using System;
using System.Linq;
using Cassette.BundleProcessing;
using Cassette.CDN;
using Cassette.Scripts;
using Cassette.Stylesheets;
using Cassette.Utilities;
using Moq;
using Should;
using Xunit;

namespace Cassette
{    
    public class BundleCollection_AddCdnUrl_Tests : BundleCollectionTestsBase
    {
        const string CdnUrl = "//cdn.test.com";
        const string BundleAlias = "testAlias";
        byte[] TestHash = new byte[] { 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00 };

        public BundleCollection_AddCdnUrl_Tests()
        {
            bundleFactoryProvider
                .Setup(f => f.GetBundleFactory<CdnScriptBundle>())
                .Returns(new CdnScriptBundleFactory(() => Mock.Of<IBundlePipeline<ScriptBundle>>()));

            bundleFactoryProvider
                .Setup(f => f.GetBundleFactory<CdnStylesheetBundle>())
                .Returns(new CdnStylesheetBundleFactory(() => Mock.Of<IBundlePipeline<StylesheetBundle>>()));
        }

        [Fact]
        public void WhenAddCdnUrlWithLocalAssets_ThenBundleHasAsset()
        {
            settings.SourceDirectory = new FakeFileSystem
            {
                "~/path/file.js",
                "~/path/file1.js"
            };

            bundles.AddCdnUrlWithLocalAssets<CdnScriptBundle>(BundleAlias, CdnUrl , new LocalAssetSettings { Path = "path" });

            bundles[BundleAlias].Assets[0].Path.ShouldEqual("~/path/file.js");
            bundles[BundleAlias].Assets[1].Path.ShouldEqual("~/path/file1.js");
        }

        [Fact]
        public void WhenAddCdnUrlWithLocalAssets_ThenBundleCanBeAccessedByAlias()
        {
            settings.SourceDirectory = new FakeFileSystem
            {
                "~/path/file.js"
            };

            bundles.AddCdnUrlWithLocalAssets<CdnScriptBundle>(BundleAlias, CdnUrl, new LocalAssetSettings { Path = "path" });
            //bundles[BundleAlias].Hash = TestHash;
            bundles[BundleAlias].ShouldBeType<CdnScriptBundle>();
        }

        [Fact]
        public void WhenAddCdnUrlWithLocalAssetsWithFileSearch_ThenFileSourceUsed()
        {
            settings.SourceDirectory = new FakeFileSystem
            {
                "~/path/file.js"
            };

            bundles.AddCdnUrlWithLocalAssets<CdnScriptBundle>(BundleAlias,  CdnUrl, new LocalAssetSettings
            {
                Path = "path", 
                FileSearch = fileSearch.Object
            });

            bundles[BundleAlias].Assets[0].Path.ShouldEqual("~/path/file.js");
        }

        [Fact]
        public void WhenAddCdnUrlWithLocalAssetsSingleFile_ThenBundleHasSingleAsset()
        {
            settings.SourceDirectory = new FakeFileSystem
            {
                "~/jquery.js"
            };

            bundles.AddCdnUrlWithLocalAssets<CdnScriptBundle>(BundleAlias, CdnUrl, new LocalAssetSettings
            {
                Path = "~/jquery.js"
            });

            var bundle = bundles["jquery.js"].ShouldBeType<CdnScriptBundle>();
            bundle.Assets[0].Path.ShouldEqual("~/jquery.js");
        }

        [Fact]
        public void WhenAddCdnUrlWithFallback_ThenExternalBundleCreatedWithFallbackCondition()
        {
            settings.SourceDirectory = new FakeFileSystem
            {
                "~/path/file.js"
            };

            bundles.AddCdnUrlWithLocalAssets<CdnScriptBundle>(BundleAlias, CdnUrl, new LocalAssetSettings
            {
                Path = "path",
                FallbackCondition = "condition"
            });

            bundles.Get<CdnScriptBundle>(BundleAlias).FallbackCondition.ShouldEqual("condition");
        }

        [Fact]
        public void WhenAddCdnUrlWithLocalAssetsWherePathIsExistingBundle_ThenReplaceTheExistingBundle()
        {
            var existingBundle = new CdnScriptBundle("~/path");
            bundles.Add(existingBundle);

            settings.SourceDirectory = new FakeFileSystem
            {
                "~/path/file.js"
            };

            bundles.AddCdnUrlWithLocalAssets<CdnScriptBundle>(BundleAlias, CdnUrl, new LocalAssetSettings
            {
                Path = "path"
            });

            bundles.Count().ShouldEqual(1);
            bundles.First().ShouldNotBeSameAs(existingBundle);
        }

        [Fact]
        public void WhenAddCdnUrlWithLocalAssets_ThenCdnUrlIsParsedCorrectly()
        {
            settings.SourceDirectory = new FakeFileSystem
            {
                "~/path/file.js",
                "~/path/file1.js"
            };

            bundles.AddCdnUrlWithLocalAssets<CdnScriptBundle>(BundleAlias, CdnUrl, new LocalAssetSettings { Path = "path" });

            var bundle = (CdnScriptBundle)bundles[BundleAlias];
            bundle.Hash = TestHash;
            bundle.ExternalUrl.ShouldEqual(String.Format("{0}/script/{1}/{2}.js", 
                CdnUrl, BundleAlias, TestHash.ToHexString()));
        }
    }
}