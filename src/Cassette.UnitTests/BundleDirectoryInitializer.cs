using System;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using Cassette.IO;
using Moq;
using Should;
using Xunit;

namespace Cassette
{
    public class BundleDirectoryInitializer_Tests : IDisposable
    {
        public BundleDirectoryInitializer_Tests()
        {
            temp = new TempDirectory();
            directory = new FileSystemDirectory(temp);
            var applicationMock = new Mock<ICassetteApplication>();
            applicationMock.SetupGet(a => a.RootDirectory).Returns(directory);
            application = applicationMock.Object;
        }

        readonly TempDirectory temp;
        readonly FileSystemDirectory directory;
        readonly ICassetteApplication application;

        [Fact]
        public void GivenSimpleFilePatternAndSomeFiles_WhenInitializeBundle_ThenAssetCreatedForEachMatchingFile()
        {
            var initializer = new BundleDirectoryInitializer("~/test")
            {
                FilePattern = "*.js"
            };

            CreateDirectory("test");
            CreateFile("test", "asset1.js");
            CreateFile("test", "asset2.js");
            CreateFile("test", "other.txt"); // this file should be ignored

            var bundle = new TestableBundle("~/test");
            initializer.InitializeBundle(bundle, application);

            var assets = bundle.Assets.OrderBy(a => a.SourceFilename).ToArray();
            assets[0].SourceFilename.ShouldEqual("~/test/asset1.js");
            assets[1].SourceFilename.ShouldEqual("~/test/asset2.js");
            assets.Length.ShouldEqual(2);
        }

        [Fact]
        public void GivenFilePatternForJSandCoffeeScript_WhenInitializeBundle_ThenBothTypesOfAssetAreCreated()
        {
            var initializer = new BundleDirectoryInitializer("~/test")
            {
                FilePattern = "*.js;*.coffee"
            };

            CreateDirectory("test");
            CreateFile("test", "asset1.js");
            CreateFile("test", "asset2.coffee");

            var bundle = new TestableBundle("~/test");
            initializer.InitializeBundle(bundle, application);

            var assets = bundle.Assets.OrderBy(a => a.SourceFilename).ToArray();
            assets[0].SourceFilename.ShouldEqual("~/test/asset1.js");
            assets[1].SourceFilename.ShouldEqual("~/test/asset2.coffee");
        }

        [Fact]
        public void GivenFilePatternForJSandCoffeeScriptUsingCommaSeparator_WhenInitializeBundle_ThenBothTypesOfAssetAreCreated()
        {
            var initializer = new BundleDirectoryInitializer("~/test")
            {
                FilePattern = "*.js,*.coffee"
            };

            CreateDirectory("test");
            CreateFile("test", "asset1.js");
            CreateFile("test", "asset2.coffee");

            var bundle = new TestableBundle("~/test");
            initializer.InitializeBundle(bundle, application);

            var assets = bundle.Assets.OrderBy(a => a.SourceFilename).ToArray();
            assets[0].SourceFilename.ShouldEqual("~/test/asset1.js");
            assets[1].SourceFilename.ShouldEqual("~/test/asset2.coffee");
        }

        [Fact]
        public void GivenFilePatternIsNotSet_WhenInitializeBundle_ThenMatchAllFiles()
        {
            var initializer = new BundleDirectoryInitializer("~/test");

            CreateDirectory("test");
            CreateFile("test", "asset1.js");
            CreateFile("test", "asset2.txt");

            var bundle = new TestableBundle("~/test");
            initializer.InitializeBundle(bundle, application);

            var assets = bundle.Assets.OrderBy(a => a.SourceFilename).ToArray();
            assets[0].SourceFilename.ShouldEqual("~/test/asset1.js");
            assets[1].SourceFilename.ShouldEqual("~/test/asset2.txt");
        }

        [Fact]
        public void GivenExclude_WhenInitializeBundle_ThenAssetsNotCreatedForFilesMatchingExclude()
        {
            var initializer = new BundleDirectoryInitializer("~/test")
            {
                ExcludeFilePath = new Regex("-vsdoc\\.js$"),
            };

            CreateDirectory("test");
            CreateFile("test", "asset.js");
            CreateFile("test", "asset-vsdoc.js");

            var bundle = new TestableBundle("~/test");
            initializer.InitializeBundle(bundle, application);
            bundle.Assets.Count.ShouldEqual(1);
        }

        [Fact]
        public void GivenBundleDescriptorFile_WhenInitializeBundle_ThenAssetIsNotCreatedForTheDescriptorFile()
        {
            var initializer = new BundleDirectoryInitializer("~/test");
            CreateDirectory("test");
            CreateFile("test", "bundle.txt");
            CreateFile("test", "module.txt"); // Legacy support - module.txt synonymous to bundle.txt

            var bundle = new TestableBundle("~/test");
            initializer.InitializeBundle(bundle, application);
            bundle.Assets.ShouldBeEmpty();
        }

        [Fact]
        public void GivenBundleDescriptorWithExplicitAssets_WhenInitializeBundle_ThenAssetsAreInDescriptorOrder()
        {
            CreateDirectory("test");
            CreateFile("test", "asset-A.js");
            CreateFile("test", "asset-Z.js");
            var descriptor = new BundleDescriptor(new[] { "asset-Z.js", "asset-A.js" });
            var initializer = new BundleDirectoryInitializer("~/test")
            {
                BundleDescriptor = descriptor
            };

            var bundle = new TestableBundle("~/test");
            initializer.InitializeBundle(bundle, application);

            bundle.Assets[0].SourceFilename.ShouldEqual("~/test/asset-Z.js");
            bundle.Assets[1].SourceFilename.ShouldEqual("~/test/asset-A.js");
        }

        [Fact]
        public void GivenBundleDescriptorIsNullButDescriptorFileExists_WhenInitializeBundle_WhenDescriptorFilesIsUsed()
        {
            CreateDirectory("test");
            CreateFile("test", "asset-A.js");
            CreateFile("test", "asset-Z.js");
            CreateFile("test", "bundle.txt")("asset-Z.js\nasset-A.js");

            var initializer = new BundleDirectoryInitializer("~/test");
            var bundle = new TestableBundle("~/test");
            initializer.InitializeBundle(bundle, application);

            bundle.Assets[0].SourceFilename.ShouldEqual("~/test/asset-Z.js");
            bundle.Assets[1].SourceFilename.ShouldEqual("~/test/asset-A.js");
        }

        public void Dispose()
        {
            temp.Dispose();
        }

        void CreateDirectory(string name)
        {
            Directory.CreateDirectory(Path.Combine(temp, name));
        }

        Action<string> CreateFile(params string[] paths)
        {
            var filename = Path.Combine(temp, Path.Combine(paths));
            File.WriteAllText(filename, "");
            return content => File.WriteAllText(filename, content);
        }
    }
}