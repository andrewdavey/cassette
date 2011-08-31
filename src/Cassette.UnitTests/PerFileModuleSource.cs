using System;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using Moq;
using Should;
using Xunit;

namespace Cassette
{
    public class PerFileModuleSource_Tests : IDisposable
    {
        readonly DirectoryInfo root;
        readonly IFileSystem fileSystem;

        public PerFileModuleSource_Tests()
        {
            root = Directory.CreateDirectory(Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString()));
            fileSystem = new FileSystem(root.FullName);
        }

        [Fact]
        public void GetModulesReturnsModuleForFile()
        {
            File.WriteAllText(Path.Combine(root.FullName, "test.js"), "");

            var modules = GetModules(new PerFileModuleSource<Module>(""));

            modules[0].Path.ShouldEqual("test.js");
        }

        [Fact]
        public void GetModulesReturnsModuleWithSingleAssetForTheFile()
        {
            File.WriteAllText(Path.Combine(root.FullName, "test.js"), "");

            var modules = GetModules(new PerFileModuleSource<Module>(""));

            modules[0].Assets.Count.ShouldEqual(1);
        }

        [Fact]
        public void GetModulesReturnsModuleWithSingleAssetWithEmptySourceFilename()
        {
            File.WriteAllText(Path.Combine(root.FullName, "test.js"), "");

            var modules = GetModules(new PerFileModuleSource<Module>(""));

            modules[0].Assets[0].SourceFilename.ShouldEqual("");
        }

        [Fact]
        public void WhenBasePathSet_ThenGetModulesReturnsModuleForFile()
        {
            root.CreateSubdirectory("path");
            File.WriteAllText(Path.Combine(root.FullName, "path\\test.js"), "");

            var modules = GetModules(new PerFileModuleSource<Module>("path"));

            modules[0].Path.ShouldEqual("path\\test.js");
        }

        [Fact]
        public void GivenFileInSubDirectory_ThenGetModulesReturnsModuleForFile()
        {
            root.CreateSubdirectory("path\\sub");
            File.WriteAllText(Path.Combine(root.FullName, "path\\sub\\test.js"), "");

            var modules = GetModules(new PerFileModuleSource<Module>("path"));

            modules[0].Path.ShouldEqual("path\\sub\\test.js");
        }

        [Fact]
        public void WhenAFilePatternSet_ThenGetModulesReturnsModuleForEachFileMatchingFilePattern()
        {
            File.WriteAllText(Path.Combine(root.FullName, "test1.js"), "");
            File.WriteAllText(Path.Combine(root.FullName, "test2.js"), "");
            File.WriteAllText(Path.Combine(root.FullName, "test3.txt"), "");

            var modules = GetModules(
                new PerFileModuleSource<Module>("")
                {
                    FilePattern = "*.js"
                }
            );

            modules.Length.ShouldEqual(2);
            modules[0].Path.ShouldEqual("test1.js");
            modules[1].Path.ShouldEqual("test2.js");
        }

        [Fact]
        public void WhenAMultipleFilePatternSet_ThenGetModulesReturnsModuleForEachFileMatchingFilePattern()
        {
            File.WriteAllText(Path.Combine(root.FullName, "test1.js"), "");
            File.WriteAllText(Path.Combine(root.FullName, "test2.coffee"), "");

            var modules = GetModules(
                new PerFileModuleSource<Module>("")
                {
                    FilePattern = "*.js;*.coffee"
                }
            );

            modules.Length.ShouldEqual(2);
            modules[0].Path.ShouldEqual("test1.js");
            modules[1].Path.ShouldEqual("test2.coffee");
        }

        [Fact]
        public void WhenExcludeSet_ThenGetModulesDoesNotCreateModuleForFileThatMatchesRegex()
        {
            File.WriteAllText(Path.Combine(root.FullName, "test1.js"), "");

            var modules = GetModules(
                new PerFileModuleSource<Module>("")
                {
                    Exclude = new Regex("test")
                }
            );

            modules.ShouldBeEmpty();
        }

        [Fact]
        public void WhenGetModules_ThenModuleAssetCanReferenceOtherFiles()
        {
            File.WriteAllText(Path.Combine(root.FullName, "test1.js"), "");
            var modules = GetModules(
                new PerFileModuleSource<Module>("")
            );
            var asset = modules[0].Assets[0];

            asset.AddReference("other.js", 1);

        }

        Module[] GetModules(PerFileModuleSource<Module> source)
        {
            var application = StubApplication();
            var factory = StubModuleFactory();
            return source.GetModules(factory, application).ToArray();
        }

        IModuleFactory<Module> StubModuleFactory()
        {
            var factory = new Mock<IModuleFactory<Module>>();
            factory
                .Setup(f => f.CreateModule(It.IsAny<string>()))
                .Returns<string>(path => new Module(path));
            return factory.Object;
        }

        ICassetteApplication StubApplication()
        {
            var application = new Mock<ICassetteApplication>();
            application
                .Setup(a => a.RootDirectory)
                .Returns(fileSystem);
            return application.Object;
        }

        public void Dispose()
        {
            root.Delete(true);
        }
    }
}
