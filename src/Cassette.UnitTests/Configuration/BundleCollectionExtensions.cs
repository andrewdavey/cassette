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
        readonly Mock<IAssetSource> defaultAssetSource;
        readonly CassetteSettings settings;

        public BundleCollection_Add_Tests()
        {
            tempDirectory = new TempDirectory();
            CreateDirectory("test");
            factory = new Mock<IBundleFactory<TestableBundle>>();
            factory.Setup(f => f.CreateBundle(It.IsAny<string>(), null))
                   .Returns<string, BundleDescriptor>((path, d) => (createdBundle = new TestableBundle(path)));
            defaultAssetSource = new Mock<IAssetSource>();
            settings = new CassetteSettings
            {
                SourceDirectory = new FileSystemDirectory(tempDirectory),
                BundleFactories = { { typeof(TestableBundle), factory.Object } },
                DefaultAssetSources = { { typeof(TestableBundle), defaultAssetSource.Object } }
            };
            bundles = new BundleCollection(settings);
        }

        [Fact]
        public void GivenDefaultAssetSourceReturnsAnAsset_WhenAddDirectoryPath_ThenBundleAddedWithTheAsset()
        {
            var asset = StubAsset();
            defaultAssetSource
                .Setup(s => s.GetAssets(It.IsAny<IDirectory>(), It.IsAny<Bundle>()))
                .Returns(new[] { asset });

            bundles.Add<TestableBundle>("~/test");

            bundles["~/test"].ShouldBeSameAs(createdBundle);
            bundles["~/test"].Assets[0].ShouldBeSameAs(asset);
        }

        [Fact]
        public void WhenAddWithDirectoryPathAndAssetSource_ThenSourceIsUsedToGetAssets()
        {
            var assetSource = new Mock<IAssetSource>();
            assetSource.Setup(s => s.GetAssets(It.IsAny<IDirectory>(), It.IsAny<Bundle>()))
                       .Returns(new[] { StubAsset() })
                       .Verifiable();

            bundles.Add<TestableBundle>("~/test", assetSource.Object);

            assetSource.Verify();
        }

        [Fact]
        public void WhenAddWithCustomizeAction_ThenCustomizeActionCalledAfterTheAssetsHaveBeenAdded()
        {
            var asset = StubAsset();
            defaultAssetSource.Setup(s => s.GetAssets(It.IsAny<IDirectory>(), It.IsAny<Bundle>()))
                       .Returns(new[] { asset })
                       .Verifiable();

            int assetCount = 0;
            Action<TestableBundle> action = b => assetCount = b.Assets.Count;

            bundles.Add("~/test", action);

            assetCount.ShouldEqual(1);
        }

        [Fact]
        public void GivenFilePath_WhenAdd_ThenBundleAddedWithSingleAsset()
        {
            File.WriteAllText(Path.Combine(tempDirectory, "file.js"), "");
            bundles.Add<TestableBundle>("~/file.js");

            bundles["~/file.js"].Assets[0].SourceFile.FullPath.ShouldEqual("~/file.js");
        }

        [Fact]
        public void GivenPathThatDoesNotExist_WhenAddWith_ThenThrowException()
        {
            Assert.Throws<DirectoryNotFoundException>(
                () => bundles.Add<TestableBundle>("~/does-not-exist")
            );
        }

        void CreateDirectory(string path)
        {
            Directory.CreateDirectory(Path.Combine(tempDirectory, path));
        }

        IAsset StubAsset()
        {
            var asset = new Mock<IAsset>();
            asset.SetupGet(a => a.SourceFile.FullPath).Returns("");
            return asset.Object;
        }

        public void Dispose()
        {
            tempDirectory.Dispose();
        }
    }

    // TODO: Rewrite these tests
    public class BundleCollectionExtensions_Tests
    {
        

        [Fact]
        public void GivenTwoBundleDirectories_WhenAddForEachSubDirectory_ThenTwoBundlesAreAddedToTheCollection()
        {
            using (var tempDirectory = new TempDirectory())
            {
                Directory.CreateDirectory(Path.Combine(tempDirectory, "test", "bundle-a"));
                Directory.CreateDirectory(Path.Combine(tempDirectory, "test", "bundle-b"));

                var settings = new CassetteSettings
                {
                    SourceDirectory = new FileSystemDirectory(tempDirectory)
                };
            
                var factory = new Mock<IBundleFactory<Bundle>>();
                var bundles = new Queue<Bundle>(new[] { new TestableBundle("~/test/bundle-a"), new TestableBundle("~/test/bundle-b") });
                factory.Setup(f => f.CreateBundle(It.IsAny<string>(), It.IsAny<BundleDescriptor>()))
                       .Returns(bundles.Dequeue);
                settings.BundleFactories[typeof(Bundle)] = factory.Object;

                var collection = new BundleCollection(settings);
                collection.AddForEachSubDirectory<Bundle>("~/test");

                var result = collection.ToArray();
                result[0].Path.ShouldEqual("~/test/bundle-a");
                result[1].Path.ShouldEqual("~/test/bundle-b");
            }
        }

        [Fact]
        public void GivenBundleDirectoryWithDescriptorFile_WhenAddForEachSubDirectory_ThenDescriptorPassedToBundleFactory()
        {
            using (var tempDirectory = new TempDirectory())
            {
                Directory.CreateDirectory(Path.Combine(tempDirectory, "test", "bundle"));
                File.WriteAllText(Path.Combine(tempDirectory, "test", "bundle", "bundle.txt"), "");
                var settings = new CassetteSettings
                {
                    SourceDirectory = new FileSystemDirectory(tempDirectory)
                };
                var factory = new Mock<IBundleFactory<Bundle>>();
                factory.Setup(f => f.CreateBundle(It.IsAny<string>(), It.IsAny<BundleDescriptor>()))
                       .Returns(new TestableBundle("~/test/bundle"));
                settings.BundleFactories[typeof(Bundle)] = factory.Object;

                var collection = new BundleCollection(settings);
                collection.AddForEachSubDirectory<Bundle>("~/test");
                
                factory.Verify(f => f.CreateBundle("~/test/bundle", It.Is<BundleDescriptor>(b => b != null)));
            }
        }

        [Fact]
        public void WhenAddForeachSubDirectoryWithBundleCustomization_ThenBundleIsCustomized()
        {
            using (var tempDirectory = new TempDirectory())
            {
                Directory.CreateDirectory(Path.Combine(tempDirectory, "test"));

                var factory = new Mock<IBundleFactory<Bundle>>();
                factory.Setup(f => f.CreateBundle(It.IsAny<string>(), It.IsAny<BundleDescriptor>()))
                       .Returns(new TestableBundle("~/test"));

                var settings = new CassetteSettings
                {
                    SourceDirectory = new FileSystemDirectory(tempDirectory),
                    BundleFactories = { { typeof(Bundle), factory.Object } }
                };
                var bundles = new BundleCollection(settings);
                
                bundles.AddForEachSubDirectory<Bundle>("~/", bundle => bundle.ContentType = "TEST");

                bundles["~/test"].ContentType.ShouldEqual("TEST");
            }
        }

        [Fact]
        public void GivenHiddenDirectory_WhenAddForEachSubDirectory_ThenDirectoryIsIgnored()
        {
            using (var tempDirectory = new TempDirectory())
            {
                Directory.CreateDirectory(Path.Combine(tempDirectory, "test", "bundle"));
                var attributes = File.GetAttributes(Path.Combine(tempDirectory, "test", "bundle"));
                File.SetAttributes(Path.Combine(tempDirectory, "test", "bundle"), attributes | FileAttributes.Hidden);
                
                var settings = new CassetteSettings
                {
                    SourceDirectory = new FileSystemDirectory(tempDirectory)
                };
                settings.BundleFactories[typeof(Bundle)] = Mock.Of<IBundleFactory<Bundle>>();

                var collection = new BundleCollection(settings);
                collection.AddForEachSubDirectory<Bundle>("~/test");

                collection.ShouldBeEmpty();
            }
        }
    }
}