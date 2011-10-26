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
            applicationMock.SetupGet(a => a.SourceDirectory).Returns(directory);
            application = applicationMock.Object;
        }

        readonly TempDirectory temp;
        readonly FileSystemDirectory directory;
        readonly ICassetteApplication application;

        [Fact]
        public void GivenSimpleFilePatternAndSomeFiles_WhenInitializeBundle_ThenAssetCreatedForEachMatchingFile()
        {
            var initializer = new BundleDirectoryInitializer()
            {
                FilePattern = "*.js"
            };

            CreateDirectory("test");
            CreateFile("test", "asset1.js");
            CreateFile("test", "asset2.js");
            CreateFile("test", "other.txt"); // this file should be ignored

            var bundle = new TestableBundle("~/test");
            initializer.InitializeBundle(bundle, application);

            var assets = bundle.Assets.OrderBy(a => a.SourceFile.FullPath).ToArray();
            assets[0].SourceFile.FullPath.ShouldEqual("~/test/asset1.js");
            assets[1].SourceFile.FullPath.ShouldEqual("~/test/asset2.js");
            assets.Length.ShouldEqual(2);
        }

        [Fact]
        public void GivenFilePatternForJSandCoffeeScript_WhenInitializeBundle_ThenBothTypesOfAssetAreCreated()
        {
            var initializer = new BundleDirectoryInitializer()
            {
                FilePattern = "*.js;*.coffee"
            };

            CreateDirectory("test");
            CreateFile("test", "asset1.js");
            CreateFile("test", "asset2.coffee");

            var bundle = new TestableBundle("~/test");
            initializer.InitializeBundle(bundle, application);

            var assets = bundle.Assets.OrderBy(a => a.SourceFile.FullPath).ToArray();
            assets[0].SourceFile.FullPath.ShouldEqual("~/test/asset1.js");
            assets[1].SourceFile.FullPath.ShouldEqual("~/test/asset2.coffee");
        }

        [Fact]
        public void GivenFilePatternForJSandCoffeeScriptUsingCommaSeparator_WhenInitializeBundle_ThenBothTypesOfAssetAreCreated()
        {
            var initializer = new BundleDirectoryInitializer()
            {
                FilePattern = "*.js,*.coffee"
            };

            CreateDirectory("test");
            CreateFile("test", "asset1.js");
            CreateFile("test", "asset2.coffee");

            var bundle = new TestableBundle("~/test");
            initializer.InitializeBundle(bundle, application);

            var assets = bundle.Assets.OrderBy(a => a.SourceFile.FullPath).ToArray();
            assets[0].SourceFile.FullPath.ShouldEqual("~/test/asset1.js");
            assets[1].SourceFile.FullPath.ShouldEqual("~/test/asset2.coffee");
        }

        [Fact]
        public void GivenFilePatternIsNotSet_WhenInitializeBundle_ThenMatchAllFiles()
        {
            var initializer = new BundleDirectoryInitializer();

            CreateDirectory("test");
            CreateFile("test", "asset1.js");
            CreateFile("test", "asset2.txt");

            var bundle = new TestableBundle("~/test");
            initializer.InitializeBundle(bundle, application);

            var assets = bundle.Assets.OrderBy(a => a.SourceFile.FullPath).ToArray();
            assets[0].SourceFile.FullPath.ShouldEqual("~/test/asset1.js");
            assets[1].SourceFile.FullPath.ShouldEqual("~/test/asset2.txt");
        }

        [Fact]
        public void GivenPathSet_WhenInitializeBundle_ThenPathIsusedInsteadOfBundlePath()
        {
            var initializer = new BundleDirectoryInitializer("~/other");
            CreateDirectory("other");
            CreateFile("other", "asset1.js");
            var bundle = new TestableBundle("~/test");

            initializer.InitializeBundle(bundle, application);

            bundle.Assets[0].SourceFile.FullPath.ShouldEqual("~/other/asset1.js");
        }

        [Fact]
        public void GivenExclude_WhenInitializeBundle_ThenAssetsNotCreatedForFilesMatchingExclude()
        {
            var initializer = new BundleDirectoryInitializer()
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
            var initializer = new BundleDirectoryInitializer();
            CreateDirectory("test");
            CreateFile("test", "bundle.txt");
            CreateFile("test", "module.txt"); // Legacy support - module.txt synonymous to bundle.txt

            var bundle = new TestableBundle("~/test");
            initializer.InitializeBundle(bundle, application);
            bundle.Assets.ShouldBeEmpty();
        }

        [Fact]
        public void GivenDescriptorFileExists_WhenInitializeBundle_ThenDescriptorFilesIsUsed()
        {
            CreateDirectory("test");
            CreateFile("test", "asset-A.js");
            CreateFile("test", "asset-Z.js");
            CreateFile("test", "bundle.txt")("asset-Z.js\nasset-A.js");

            var initializer = new BundleDirectoryInitializer();
            var bundle = new TestableBundle("~/test");
            initializer.InitializeBundle(bundle, application);

            bundle.Assets[0].SourceFile.FullPath.ShouldEqual("~/test/asset-Z.js");
            bundle.Assets[1].SourceFile.FullPath.ShouldEqual("~/test/asset-A.js");
        }

        [Fact]
        public void GivenDescriptorUsedToCreateBundle_WhenSorted_BundleAssetOrderIsUnchanged()
        {
            CreateDirectory("test");
            CreateFile("test", "asset-A.js");
            CreateFile("test", "asset-B.js");
            CreateFile("test", "asset-C.js");
            CreateFile("test", "bundle.txt")("asset-C.js\nasset-A.js\nasset-B.js");

            var initializer = new BundleDirectoryInitializer();
            var bundle = new TestableBundle("~/test");
            initializer.InitializeBundle(bundle, application);
            // The following reference should be ignored, because we're building from a descriptor.
            bundle.Assets[0].AddReference("~/test/asset-B.js", -1);

            bundle.SortAssetsByDependency();

            bundle.Assets[0].SourceFile.FullPath.ShouldEqual("~/test/asset-C.js");
            bundle.Assets[1].SourceFile.FullPath.ShouldEqual("~/test/asset-A.js");
            bundle.Assets[2].SourceFile.FullPath.ShouldEqual("~/test/asset-B.js");
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