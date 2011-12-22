using System;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using Cassette.IO;
using Should;
using Xunit;

namespace Cassette.Configuration
{
    public class FileSearch_Tests : IDisposable
    {
        public FileSearch_Tests()
        {
            temp = new TempDirectory();
            directory = new FileSystemDirectory(temp);
        }

        readonly TempDirectory temp;
        readonly FileSystemDirectory directory;

        [Fact]
        public void GivenSimpleFilePatternAndSomeFiles_WhenFindFiles_ThenAssetCreatedForEachMatchingFile()
        {
            var search = new FileSearch
            {
                Pattern = "*.js"
            };

            CreateDirectory("test");
            CreateFile("test", "asset1.js");
            CreateFile("test", "asset2.js");
            CreateFile("test", "other.txt"); // this file should be ignored

            var files = search.FindFiles(directory.GetDirectory("test"))
                               .OrderBy(f => f.FullPath).ToArray();
            
            files[0].FullPath.ShouldEqual("~/test/asset1.js");
            files[1].FullPath.ShouldEqual("~/test/asset2.js");
            files.Length.ShouldEqual(2);
        }

        [Fact]
        public void GivenFilePatternForJSandCoffeeScript_WhenFindFiles_ThenBothTypesOfAssetAreCreated()
        {
            var search = new FileSearch
            {
                Pattern = "*.js;*.coffee"
            };

            CreateDirectory("test");
            CreateFile("test", "asset1.js");
            CreateFile("test", "asset2.coffee");

            var files = search.FindFiles(directory.GetDirectory("test"))
                               .OrderBy(f => f.FullPath).ToArray();

            files[0].FullPath.ShouldEqual("~/test/asset1.js");
            files[1].FullPath.ShouldEqual("~/test/asset2.coffee");
        }

        [Fact]
        public void GivenFilePatternForJSandCoffeeScriptUsingCommaSeparator_WhenFindFiles_ThenBothTypesOfAssetAreCreated()
        {
            var search = new FileSearch()
            {
                Pattern = "*.js,*.coffee"
            };

            CreateDirectory("test");
            CreateFile("test", "asset1.js");
            CreateFile("test", "asset2.coffee");

            var files = search.FindFiles(directory.GetDirectory("test"))
                               .OrderBy(f => f.FullPath).ToArray();

            files[0].FullPath.ShouldEqual("~/test/asset1.js");
            files[1].FullPath.ShouldEqual("~/test/asset2.coffee");
        }

        [Fact]
        public void GivenFilePatternIsNotSet_WhenFindFiles_ThenMatchAllFiles()
        {
            var search = new FileSearch();

            CreateDirectory("test");
            CreateFile("test", "asset1.js");
            CreateFile("test", "asset2.txt");

            var files = search.FindFiles(directory.GetDirectory("test"))
                               .OrderBy(f => f.FullPath).ToArray();

            files[0].FullPath.ShouldEqual("~/test/asset1.js");
            files[1].FullPath.ShouldEqual("~/test/asset2.txt");
        }

        [Fact]
        public void GivenExclude_WhenFindFiles_ThenAssetsNotCreatedForFilesMatchingExclude()
        {
            var search = new FileSearch()
            {
                Exclude = new Regex("-vsdoc\\.js$"),
            };

            CreateDirectory("test");
            CreateFile("test", "asset.js");
            CreateFile("test", "asset-vsdoc.js");

            var files = search.FindFiles(directory.GetDirectory("test"));

            files.Count().ShouldEqual(1);
        }

        [Fact]
        public void GivenBundleDescriptorFile_WhenFindFiles_ThenAssetIsNotCreatedForTheDescriptorFile()
        {
            var search = new FileSearch();
            CreateDirectory("test");
            CreateFile("test", "bundle.txt");
            CreateFile("test", "module.txt"); // Legacy support - module.txt synonymous to bundle.txt

            var files = search.FindFiles(directory.GetDirectory("test"));
            
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
