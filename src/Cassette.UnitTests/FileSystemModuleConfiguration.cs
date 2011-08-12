using System;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using Moq;
using Should;
using Xunit;

namespace Cassette
{
    public class FileSystemModuleConfiguration_TestsWithFiles : IDisposable
    {
        public FileSystemModuleConfiguration_TestsWithFiles()
        {
            // Create a basic set of directories and files.
            root = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString());
            Directory.CreateDirectory(Path.Combine(root));
            Directory.CreateDirectory(Path.Combine(root, "scripts"));
            Directory.CreateDirectory(Path.Combine(root, "scripts", "module-a"));
            File.WriteAllText(Path.Combine(root, "scripts", "module-a", "test-1.js"), "test-1");
            File.WriteAllText(Path.Combine(root, "scripts", "module-a", "test-2.js"), "test-2");
            File.WriteAllText(Path.Combine(root, "scripts", "module-a", "test-2.vsdoc.js"), "test-2");
            Directory.CreateDirectory(Path.Combine(root, "scripts", "module-b"));
            File.WriteAllText(Path.Combine(root, "scripts", "module-b", "test-3.js"), "test-3");
            File.WriteAllText(Path.Combine(root, "scripts", "module-b", "ignore.me"), "");
            // Hidden directories should be ignored by ForSubDirectoriesOf
            var svn = Directory.CreateDirectory(Path.Combine(root, "scripts", ".svn"));
            svn.Attributes |= FileAttributes.Hidden;
            Directory.CreateDirectory(Path.Combine(root, "html-templates"));
            File.WriteAllText(Path.Combine(root, "html-templates", "test-4.htm"), "");
            File.WriteAllText(Path.Combine(root, "html-templates", "test-5.html"), "");
            var fileSystem = new FileSystem(root);

            getFullPath = (path) => Path.Combine(root, path);

            var moduleFactory = new Mock<IModuleFactory<Module>>();
            moduleFactory.Setup(f => f.CreateModule(It.IsAny<string>()))
                .Returns<string>(directory => new Module(directory));
            
            cache = new Mock<IModuleCache<Module>>();

            application = new Mock<ICassetteApplication>();
            application.SetupGet(a => a.RootDirectory).Returns(fileSystem);
            application.Setup(a => a.GetModuleFactory<Module>()).Returns(moduleFactory.Object);
            application.Setup(a => a.GetModuleCache<Module>()).Returns(cache.Object);
            pipeline = new Mock<ModuleProcessing.IModuleProcessor<Module>>();

            config = new FileSystemModuleConfiguration<Module>(application.Object);
            config.ProcessWith(pipeline.Object);
        }

        public FileSystemModuleConfiguration<Module> config;
        public string root;
        public Func<string, string> getFullPath;
        public Mock<ModuleProcessing.IModuleProcessor<Module>> pipeline;
        public Mock<IModuleCache<Module>> cache;
        public Mock<ICassetteApplication> application;

        public void Dispose()
        {
            Directory.Delete(root, true);
        }
    }

    public class FileSystemModuleConfiguration_ModuleContainerBuilderTests : FileSystemModuleConfiguration_TestsWithFiles
    {
        IModuleContainer<Module> ModuleContainer
        {
            get { return (config as IModuleContainerFactory<Module>).CreateModuleContainer(); }
        }

        [Fact]
        public void WhenNoDirectoriesSpecified_ThenModuleContainerHasOneModuleForEntireRoot()
        {
            var module = ModuleContainer.Modules.Single();
            module.Directory.ShouldEqual("");
        }

        [Fact]
        public void WhenForSubDirectoriesOfScripts_ThenModuleContainerHasTwoModules()
        {
            config.ForSubDirectoriesOf("scripts");

            ModuleContainer.Modules.Count().ShouldEqual(2);
        }

        [Fact]
        public void WhenForSubDirectoriesOfScripts_ThenModulesHaveAssetsForAllFiles()
        {
            config.ForSubDirectoriesOf("scripts");

            ModuleContainer.Modules.ElementAt(0).Assets.Count.ShouldEqual(3);
            ModuleContainer.Modules.ElementAt(1).Assets.Count.ShouldEqual(2);
        }

        [Fact]
        public void WhenForSubDirectoriesOfNonexistentPath_ThenThrowDirectoryNotFoundException()
        {
            Assert.Throws<DirectoryNotFoundException>(delegate
            {
                config.ForSubDirectoriesOf("not-found");
            });
        }

        [Fact]
        public void WhenDirectoriesScriptsModuleA_ThenModuleContainerHasOneModule()
        {
            config.Directories("scripts/module-a");

            ModuleContainer.Modules.Count().ShouldEqual(1);
        }

        [Fact]
        public void WhenDirectoriesScriptsModuleAAndModuleB_ThenModuleContainerHasTwoModule()
        {
            config.Directories("scripts/module-a", "scripts/module-b");

            ModuleContainer.Modules.Count().ShouldEqual(2);
        }

        [Fact]
        public void WhenDirectoriesWithNonexistentPath_ThenThrowDirectoryNotFoundException()
        {
            Assert.Throws<DirectoryNotFoundException>(delegate
            {
                config.Directories("not-found");
            });
        }

        [Fact]
        public void WhenIncludeFilesJS_ThenModuleAssetsAreOnlyJSFiles()
        {
            config.Directories("scripts/module-b")
                  .IncludeFiles("*.js");

            var assets = ModuleContainer.Modules.First().Assets;
            assets.Count.ShouldEqual(1);
            assets[0].SourceFilename.EndsWith("test-3.js").ShouldBeTrue();
        }

        [Fact]
        public void WhenIncludeFilesHTMandHTML_ThenDoNotIncludeSameFileTwice()
        {
            config.Directories("html-templates")
                  .IncludeFiles("*.htm", "*.html");

            ModuleContainer.Modules.First().Assets.Count.ShouldEqual(2);
        }

        [Fact]
        public void WhenIncludeFilesEmptyArray_ThenAllFilesAreIncluded()
        {
            config.Directories("scripts/module-b")
                  .IncludeFiles();

            ModuleContainer.Modules.First().Assets.Count.ShouldEqual(2);
        }

        [Fact]
        public void WhenExcludeFilesByRegex_ThenModuleDoesNotHaveAssetMatching()
        {
            config.Directories("scripts/module-a")
                  .IncludeFiles("*.js")
                  .ExcludeFiles(new Regex("\\.vsdoc\\.js$"));
            ModuleContainer.Modules.First().Assets.Count.ShouldEqual(2);
        }
    }

    public class FileSystemModuleConfiguration_Interactions : FileSystemModuleConfiguration_TestsWithFiles
    {
        [Fact]
        public void WhenCreateModuleContainer_ThenPipelineProcessesEachModule()
        {
            config.ForSubDirectoriesOf("scripts");
            (config as IModuleContainerFactory<Module>).CreateModuleContainer();

            pipeline.Verify(p => p.Process(It.IsAny<Module>(), It.IsAny<ICassetteApplication>()), Times.Exactly(2));
        }

        [Fact]
        public void WhenCacheIsUpToDate_ThenPipelineIsNotProcessed()
        {
            application.SetupGet(a => a.IsOutputOptimized).Returns(true);
            cache.Setup(c => c.IsUpToDate(It.IsAny<DateTime>(), It.IsAny<string>())).Returns(true);
            (config as IModuleContainerFactory<Module>).CreateModuleContainer();

            pipeline.Verify(
                p => p.Process(It.IsAny<Module>(), It.IsAny<ICassetteApplication>()),
                Times.Never()
            );
        }
    }
}
