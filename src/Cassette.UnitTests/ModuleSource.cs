using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Moq;
using Should;
using Xunit;
using System.Text.RegularExpressions;

namespace Cassette
{
    public class ModuleSource_BehaviorTests : IDisposable
    {
        public ModuleSource_BehaviorTests()
        {
            // Create a basic set of directories and files.
            root = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString(), "scripts");
            Directory.CreateDirectory(Path.Combine(root));
                Directory.CreateDirectory(Path.Combine(root, "module-a"));
                    File.WriteAllText(Path.Combine(root, "module-a", "test-1.js"), "test-1");
                    File.WriteAllText(Path.Combine(root, "module-a", "test-2.js"), "test-2");
                Directory.CreateDirectory(Path.Combine(root, "module-b"));
                    File.WriteAllText(Path.Combine(root, "module-b", "test-3.js"), "test-3");
                    File.WriteAllText(Path.Combine(root, "module-b", "ignore.me"), "");
                // Hidden directories should be ignored by AddEachSubDirectory
                var svn = Directory.CreateDirectory(Path.Combine(root, ".svn"));
                svn.Attributes |= FileAttributes.Hidden;
            
            getFullPath = (path) => Path.Combine(root, path);

            source = new ModuleSource<Module>(root, "*.js");
        }

        ModuleSource<Module> source;
        string root;
        Func<string, string> getFullPath;

        [Fact]
        public void WhenAddDirectoryRelativePath_ThenCreateModulesReturnsModuleWithAbsolutePath()
        {
            source.AddDirectory("module-a");

            var moduleFactory = new Mock<IModuleFactory<Module>>();
            moduleFactory.Setup(f => f.CreateModule(It.IsAny<string>()))
                         .Returns(new Module("module-a", getFullPath));

            var module = source.CreateModules(moduleFactory.Object).First();

            module.Directory.ShouldEqual("module-a");
        }

        [Fact]
        public void WhenAddDirectoryRelativePath_ThenCreateModulesUsesModuleFactory()
        {
            source.AddDirectory("module-a");

            var moduleFactory = new Mock<IModuleFactory<Module>>();
            moduleFactory.Setup(f => f.CreateModule("module-a"))
                         .Returns(new Module("module-a", getFullPath))
                         .Verifiable();

            var module = source.CreateModules(moduleFactory.Object).First();

            moduleFactory.Verify();
        }

        [Fact]
        public void WhenAddDirectoryTwice_ThenCreateModulesReturnsTwoModules()
        {
            source.AddDirectory("module-a");
            source.AddDirectory("module-b");
            var moduleFactory = new Mock<IModuleFactory<Module>>();
            var stubs = new Queue<Module>(new[] { new Module("module-a", getFullPath), new Module("module-b", getFullPath) });
            moduleFactory.Setup(f => f.CreateModule(It.IsAny<string>()))
                         .Returns(() => stubs.Dequeue());

            var modules = source.CreateModules(moduleFactory.Object);

            modules.Count().ShouldEqual(2);
        }

        [Fact]
        public void WhenAddDirectoryThatDoesntExist_ThenThrowDirectoryNotFoundException()
        {
            Assert.Throws<DirectoryNotFoundException>(delegate
            {
                source.AddDirectory("module-c");
            });
        }

        [Fact]
        public void WhenAddTwoDirectories_ThenCreateModulesReturnsTwoModules()
        {
            source.AddDirectories("module-a", "module-b");
            var moduleFactory = new Mock<IModuleFactory<Module>>();
            var stubs = new Queue<Module>(new[] { new Module("module-a", getFullPath), new Module("module-b", getFullPath) });
            moduleFactory.Setup(f => f.CreateModule(It.IsAny<string>()))
                         .Returns(() => stubs.Dequeue());

            var modules = source.CreateModules(moduleFactory.Object);

            modules.Count().ShouldEqual(2);
        }

        [Fact]
        public void WhenAddDirectoriesThatDontExist_ThenThrowDirectoryNotFoundException()
        {
            Assert.Throws<DirectoryNotFoundException>(delegate
            {
                source.AddDirectories("module-c");
            });
        }

        [Fact]
        public void WhenAddEachSubDirectory_ThenCreateModulesReturnsModulePerDirectory()
        {
            source.AddEachSubDirectory();

            var moduleFactory = new Mock<IModuleFactory<Module>>();
            var stubs = new Queue<Module>(new[] { new Module("module-a", getFullPath), new Module("module-b", getFullPath) });
            moduleFactory.Setup(f => f.CreateModule(It.IsAny<string>()))
                         .Returns(() => stubs.Dequeue());

            var modules = source.CreateModules(moduleFactory.Object).ToArray();

            modules.Length.ShouldEqual(2);
            var names = modules.Select(m => m.Directory).OrderBy(d => d).ToArray();
            names[0].EndsWith("module-a").ShouldBeTrue();
            names[1].EndsWith("module-b").ShouldBeTrue();
        }

        [Fact]
        public void WhenAddDirectoryWithTwoFilesWithValidExtension_ThenCreateModulesReturnsModuleWithTwoAssets()
        {
            source.AddDirectory("module-a");
            var moduleFactory = new Mock<IModuleFactory<Module>>();
            moduleFactory.Setup(f => f.CreateModule(It.IsAny<string>()))
                         .Returns(new Module("module-a", getFullPath));

            var modules = source.CreateModules(moduleFactory.Object).ToArray();
            modules[0].Assets.Count.ShouldEqual(2);
        }

        [Fact]
        public void WhenAddDirectoryWithTwoFilesWithOnlyOneHavingValidExtension_ThenCreateModulesReturnsModuleWithOneAsset()
        {
            source.AddDirectory("module-b");
            var moduleFactory = new Mock<IModuleFactory<Module>>();
            moduleFactory.Setup(f => f.CreateModule(It.IsAny<string>()))
                         .Returns(new Module("module-b", getFullPath));

            var modules = source.CreateModules(moduleFactory.Object).ToArray();
            modules[0].Assets.Count.ShouldEqual(1);
        }

        [Fact]
        public void WhenAsSingleModule_ThenCreateModulesReturnsOneModule()
        {
            source.AsSingleModule();
            var moduleFactory = new Mock<IModuleFactory<Module>>();
            moduleFactory.Setup(f => f.CreateModule(It.IsAny<string>()))
                         .Returns(new Module("", getFullPath));
            
            var modules = source.CreateModules(moduleFactory.Object).ToArray();
            modules.Length.ShouldEqual(1);
        }

        [Fact]
        public void WhenAsSingleModule_ThenAddDirectoryThrowsInvalidOperationException()
        {
            source.AsSingleModule();
            Assert.Throws<InvalidOperationException>(() => source.AddDirectory("module-a"));
        }

        [Fact]
        public void WhenAsSingleModule_ThenAddDirectoriesThrowsInvalidOperationException()
        {
            source.AsSingleModule();
            Assert.Throws<InvalidOperationException>(() => source.AddDirectories("module-a", "module-b"));
        }

        [Fact]
        public void WhenAsSingleModule_ThenAddEachSubDirectoryThrowsInvalidOperationException()
        {
            source.AsSingleModule();
            Assert.Throws<InvalidOperationException>(() => source.AddEachSubDirectory());
        }

        [Fact]
        public void WhenDirectoriesAdded_ThenAsSingleModuleThrowsInvalidOperationException()
        {
            source.AddDirectory("module-a");
            Assert.Throws<InvalidOperationException>(() => source.AsSingleModule());
        }

        [Fact]
        public void WhenIgnoreFilesMatchingRegex_ThenCreateModulesDoesNotIncludeAssetsForIgnoredFiles()
        {
            source.IgnoreFilesMatching(new Regex("2.js$"));
            
            source.AsSingleModule();
            var moduleFactory = new Mock<IModuleFactory<Module>>();
            moduleFactory.Setup(f => f.CreateModule(It.IsAny<string>()))
                         .Returns(new Module("", getFullPath));

            var module = source.CreateModules(moduleFactory.Object).First();
            module.Assets.Count.ShouldEqual(2);
        }

        [Fact]
        public void WhenTwoIgnoreFilesMatchingRegex_ThenCreateModulesDoesNotIncludeAssetsForIgnoredFiles()
        {
            source.IgnoreFilesMatching(new Regex("2.js$"))
                  .IgnoreFilesMatching(new Regex("1.js$"));

            source.AsSingleModule();
            var moduleFactory = new Mock<IModuleFactory<Module>>();
            moduleFactory.Setup(f => f.CreateModule(It.IsAny<string>()))
                         .Returns(new Module("", getFullPath));

            var module = source.CreateModules(moduleFactory.Object).First();

            module.Assets.Count.ShouldEqual(1);
            using (var reader = new StreamReader(module.Assets[0].OpenStream()))
            {
                reader.ReadToEnd().ShouldEqual("test-3");
            }
        }

        [Fact]
        public void CreateModulesReturnsModuleContainerThatIsUpToDateWithRecentAssetFileChange()
        {
            source.AsSingleModule();
            var moduleFactory = new Mock<IModuleFactory<Module>>();
            moduleFactory.Setup(f => f.CreateModule(""))
                         .Returns(new Module("", getFullPath));

            var container = source.CreateModules(moduleFactory.Object);

            var expected = File.GetLastWriteTimeUtc(Path.Combine(root, "module-b", "test-3.js"));
            container.LastWriteTime.ShouldEqual(expected);
        }

        void IDisposable.Dispose()
        {
            Directory.Delete(root, true);
        }
    }

    public class ModuleSource_HasFluentApi : IDisposable
    {
        public ModuleSource_HasFluentApi()
        {
            // Create a basic set of directories and files.
            root = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString());
            Directory.CreateDirectory(Path.Combine(root, "scripts"));
                Directory.CreateDirectory(Path.Combine(root, "scripts", "module-a"));
                Directory.CreateDirectory(Path.Combine(root, "scripts", "module-b"));

            source = new ModuleSource<Module>(Path.Combine(root, "scripts"), "*.js");
        }

        ModuleSource<Module> source;
        string root;

        [Fact]
        public void AddDirectory_ReturnsModuleSource()
        {
            source.AddDirectory("module-a").ShouldBeSameAs(source);
        }

        [Fact]
        public void AddDirectories_ReturnsModuleSource()
        {
            source.AddDirectories("module-a", "module-b").ShouldBeSameAs(source);
        }

        [Fact]
        public void AddEachSubDirectory_ReturnsModuleSource()
        {
            source.AddEachSubDirectory().ShouldBeSameAs(source);
        }

        [Fact]
        public void AsSingleModule_ReturnsModuleSource()
        {
            source.AsSingleModule().ShouldBeSameAs(source);
        }

        [Fact]
        public void IgnoreFilesMatching_ReturnsModuleSource()
        {
            source.IgnoreFilesMatching(new Regex("")).ShouldBeSameAs(source);
        }

        void IDisposable.Dispose()
        {
            Directory.Delete(root, true);
        }
    }

    public class ModuleSource_ConstructorConstraints
    {
        [Fact]
        public void RootDirectoryMustNotBeNull()
        {
            Assert.Throws<ArgumentNullException>(delegate
            {
                new ModuleSource<Module>(null, "*.js");
            });
        }

        [Fact]
        public void RootDirectoryMustBeAbsolutePath()
        {
            Assert.Throws<ArgumentException>(delegate
            {
                new ModuleSource<Module>("foo\\bar", "*.js");
            });
        }

        [Fact]
        public void AtLeastOneAssetFileExtensionsRequired()
        {
            Assert.Throws<ArgumentException>(delegate
            {
                new ModuleSource<Module>("c:\\");
            });
            Assert.Throws<ArgumentException>(delegate
            {
                new ModuleSource<Module>("c:\\", (string[])null);
            });
        }

        [Fact]
        public void AssetFileExtensionsCannotBeNullOrEmpty()
        {
            Assert.Throws<ArgumentException>(delegate
            {
                new ModuleSource<Module>("c:\\", "");
            });

            Assert.Throws<ArgumentException>(delegate
            {
                new ModuleSource<Module>("c:\\", (string)null);
            });
        }
    }
}
