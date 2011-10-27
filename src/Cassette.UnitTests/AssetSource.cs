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
    public class AssetSource_Tests : IDisposable
    {
        public AssetSource_Tests()
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
            var source = new AssetSource
            {
                FilePattern = "*.js"
            };

            CreateDirectory("test");
            CreateFile("test", "asset1.js");
            CreateFile("test", "asset2.js");
            CreateFile("test", "other.txt"); // this file should be ignored

            var bundle = new TestableBundle("~/test");
            var assets = source.GetAssets(directory.GetDirectory("test"), bundle)
                               .OrderBy(a => a.SourceFile.FullPath).ToArray();
            
            assets[0].SourceFile.FullPath.ShouldEqual("~/test/asset1.js");
            assets[1].SourceFile.FullPath.ShouldEqual("~/test/asset2.js");
            assets.Length.ShouldEqual(2);
        }

        [Fact]
        public void GivenFilePatternForJSandCoffeeScript_WhenInitializeBundle_ThenBothTypesOfAssetAreCreated()
        {
            var source = new AssetSource
            {
                FilePattern = "*.js;*.coffee"
            };

            CreateDirectory("test");
            CreateFile("test", "asset1.js");
            CreateFile("test", "asset2.coffee");

            var bundle = new TestableBundle("~/test");
            var assets = source.GetAssets(directory.GetDirectory("test"), bundle)
                               .OrderBy(a => a.SourceFile.FullPath).ToArray();

            assets[0].SourceFile.FullPath.ShouldEqual("~/test/asset1.js");
            assets[1].SourceFile.FullPath.ShouldEqual("~/test/asset2.coffee");
        }

        [Fact]
        public void GivenFilePatternForJSandCoffeeScriptUsingCommaSeparator_WhenInitializeBundle_ThenBothTypesOfAssetAreCreated()
        {
            var source = new AssetSource()
            {
                FilePattern = "*.js,*.coffee"
            };

            CreateDirectory("test");
            CreateFile("test", "asset1.js");
            CreateFile("test", "asset2.coffee");

            var bundle = new TestableBundle("~/test");
            var assets = source.GetAssets(directory.GetDirectory("test"), bundle)
                               .OrderBy(a => a.SourceFile.FullPath).ToArray();

            assets[0].SourceFile.FullPath.ShouldEqual("~/test/asset1.js");
            assets[1].SourceFile.FullPath.ShouldEqual("~/test/asset2.coffee");
        }

        [Fact]
        public void GivenFilePatternIsNotSet_WhenInitializeBundle_ThenMatchAllFiles()
        {
            var source = new AssetSource();

            CreateDirectory("test");
            CreateFile("test", "asset1.js");
            CreateFile("test", "asset2.txt");

            var bundle = new TestableBundle("~/test");
            var assets = source.GetAssets(directory.GetDirectory("test"), bundle)
                               .OrderBy(a => a.SourceFile.FullPath).ToArray();

            assets[0].SourceFile.FullPath.ShouldEqual("~/test/asset1.js");
            assets[1].SourceFile.FullPath.ShouldEqual("~/test/asset2.txt");
        }

        [Fact]
        public void GivenExclude_WhenInitializeBundle_ThenAssetsNotCreatedForFilesMatchingExclude()
        {
            var source = new AssetSource()
            {
                ExcludeFilePath = new Regex("-vsdoc\\.js$"),
            };

            CreateDirectory("test");
            CreateFile("test", "asset.js");
            CreateFile("test", "asset-vsdoc.js");

            var bundle = new TestableBundle("~/test");
            var assets = source.GetAssets(directory.GetDirectory("test"), bundle);

            assets.Count().ShouldEqual(1);
        }

        [Fact]
        public void GivenBundleDescriptorFile_WhenInitializeBundle_ThenAssetIsNotCreatedForTheDescriptorFile()
        {
            var source = new AssetSource();
            CreateDirectory("test");
            CreateFile("test", "bundle.txt");
            CreateFile("test", "module.txt"); // Legacy support - module.txt synonymous to bundle.txt

            var bundle = new TestableBundle("~/test");
            var assets = source.GetAssets(directory.GetDirectory("test"), bundle);
            
            assets.ShouldBeEmpty();
        }

        public void Dispose()
        {
            temp.Dispose();
        }

        void CreateDirectory(string name)
        {
            Directory.CreateDirectory(Path.Combine(temp, name));
        }

        void CreateFile(params string[] paths)
        {
            var filename = Path.Combine(temp, Path.Combine(paths));
            File.WriteAllText(filename, "");
        }
    }
}