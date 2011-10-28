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
        public void GivenSimpleFilePatternAndSomeFiles_WhenGetFiles_ThenAssetCreatedForEachMatchingFile()
        {
            var source = new FileSource
            {
                FilePattern = "*.js"
            };

            CreateDirectory("test");
            CreateFile("test", "asset1.js");
            CreateFile("test", "asset2.js");
            CreateFile("test", "other.txt"); // this file should be ignored

            var files = source.GetFiles(directory.GetDirectory("test"))
                               .OrderBy(f => f.FullPath).ToArray();
            
            files[0].FullPath.ShouldEqual("~/test/asset1.js");
            files[1].FullPath.ShouldEqual("~/test/asset2.js");
            files.Length.ShouldEqual(2);
        }

        [Fact]
        public void GivenFilePatternForJSandCoffeeScript_WhenGetFiles_ThenBothTypesOfAssetAreCreated()
        {
            var source = new FileSource
            {
                FilePattern = "*.js;*.coffee"
            };

            CreateDirectory("test");
            CreateFile("test", "asset1.js");
            CreateFile("test", "asset2.coffee");

            var files = source.GetFiles(directory.GetDirectory("test"))
                               .OrderBy(f => f.FullPath).ToArray();

            files[0].FullPath.ShouldEqual("~/test/asset1.js");
            files[1].FullPath.ShouldEqual("~/test/asset2.coffee");
        }

        [Fact]
        public void GivenFilePatternForJSandCoffeeScriptUsingCommaSeparator_WhenGetFiles_ThenBothTypesOfAssetAreCreated()
        {
            var source = new FileSource()
            {
                FilePattern = "*.js,*.coffee"
            };

            CreateDirectory("test");
            CreateFile("test", "asset1.js");
            CreateFile("test", "asset2.coffee");

            var files = source.GetFiles(directory.GetDirectory("test"))
                               .OrderBy(f => f.FullPath).ToArray();

            files[0].FullPath.ShouldEqual("~/test/asset1.js");
            files[1].FullPath.ShouldEqual("~/test/asset2.coffee");
        }

        [Fact]
        public void GivenFilePatternIsNotSet_WhenGetFiles_ThenMatchAllFiles()
        {
            var source = new FileSource();

            CreateDirectory("test");
            CreateFile("test", "asset1.js");
            CreateFile("test", "asset2.txt");

            var files = source.GetFiles(directory.GetDirectory("test"))
                               .OrderBy(f => f.FullPath).ToArray();

            files[0].FullPath.ShouldEqual("~/test/asset1.js");
            files[1].FullPath.ShouldEqual("~/test/asset2.txt");
        }

        [Fact]
        public void GivenExclude_WhenGetFiles_ThenAssetsNotCreatedForFilesMatchingExclude()
        {
            var source = new FileSource()
            {
                ExcludeFilePath = new Regex("-vsdoc\\.js$"),
            };

            CreateDirectory("test");
            CreateFile("test", "asset.js");
            CreateFile("test", "asset-vsdoc.js");

            var files = source.GetFiles(directory.GetDirectory("test"));

            files.Count().ShouldEqual(1);
        }

        [Fact]
        public void GivenBundleDescriptorFile_WhenGetFiles_ThenAssetIsNotCreatedForTheDescriptorFile()
        {
            var source = new FileSource();
            CreateDirectory("test");
            CreateFile("test", "bundle.txt");
            CreateFile("test", "module.txt"); // Legacy support - module.txt synonymous to bundle.txt

            var files = source.GetFiles(directory.GetDirectory("test"));
            
            files.ShouldBeEmpty();
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