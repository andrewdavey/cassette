using System;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using Moq;
using Should;
using Xunit;

namespace Cassette
{
    public class PerSubDirectorySource_Test : IDisposable
    {
        public PerSubDirectorySource_Test()
        {
            root = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString());
            Directory.CreateDirectory(root);

            application = new Mock<ICassetteApplication>();
            application.Setup(app => app.RootDirectory)
                       .Returns(new FileSystem(root));
            moduleFactory = StubModuleFactory();
        }

        readonly string root;
        readonly Mock<ICassetteApplication> application;
        readonly IModuleFactory<Module> moduleFactory;

        [Fact]
        public void GivenBaseDirectoryHasEmptyDirectory_ThenGetModulesReturnsEmptyModule()
        {
            Directory.CreateDirectory(Path.Combine(root, "scripts", "empty"));

            var source = new PerSubDirectorySource<Module>("scripts");
            var result = source.GetModules(moduleFactory, application.Object);

            var module = result.Modules.First();
            module.Assets.Count.ShouldEqual(0);
        }

        [Fact]
        public void GivenBaseDirectoryWithTwoDirectories_ThenGetModulesReturnsTwoModules()
        {
            GivenFiles("scripts/module-a/1.js", "scripts/module-b/2.js");

            var source = new PerSubDirectorySource<Module>("scripts");
            var result = source.GetModules(moduleFactory, application.Object);
            
            var modules = result.Modules.ToArray();
            modules.Length.ShouldEqual(2);
        }

        [Fact]
        public void GivenMixedFileTypes_WhenFilesFiltered_ThenGetModulesFindsOnlyMatchingFiles()
        {
            GivenFiles("scripts/module-a/1.js", "scripts/module-a/ignored.txt");

            var source = new PerSubDirectorySource<Module>("scripts", "*.js");
            var result = source.GetModules(moduleFactory, application.Object);

            var module = result.Modules.First();
            module.Assets.Count.ShouldEqual(1);
        }

        [Fact]
        public void GivenAmbiguousFileFilters_ThenGetModulesFindsFileOnlyOnce()
        {
            GivenFiles("scripts/module-a/1.html");

            var source = new PerSubDirectorySource<Module>("scripts", "*.htm", "*.html");
            var result = source.GetModules(moduleFactory, application.Object);

            var module = result.Modules.First();
            module.Assets.Count.ShouldEqual(1);
        }

        [Fact]
        public void GivenFilesWeDontWantInModule_WhenExclusionProvided_ThenGetModulesDoesntIncludeExcludedFiles()
        {
            GivenFiles("scripts/module-a/1.js", "scripts/module-a/1-vsdoc.js");
            
            var source = new PerSubDirectorySource<Module>("scripts", "*.js");
            source.Exclude = new Regex("-vsdoc\\.js$");

            var result = source.GetModules(moduleFactory, application.Object);

            var module = result.Modules.First();
            module.Assets.Count.ShouldEqual(1);
        }

        [Fact]
        public void GivenBaseDirectoryDoesNotExist_ThenGetModulesThrowsException()
        {
            var source = new PerSubDirectorySource<Module>("missing");
            Assert.Throws<DirectoryNotFoundException>(delegate
            {
                source.GetModules(moduleFactory, application.Object);
            });
        }

        void GivenFiles(params string[] filenames)
        {
            foreach (var filename in filenames)
            {
                var dir = Path.Combine(root, Path.GetDirectoryName(filename));
                if (!Directory.Exists(dir))
                {
                    Directory.CreateDirectory(dir);
                }
                File.WriteAllText(Path.Combine(root, filename), "");
            }
        }

        IModuleFactory<Module> StubModuleFactory()
        {
            var factory = new Mock<IModuleFactory<Module>>();
            factory.Setup(f => f.CreateModule(It.IsAny<string>()))
                   .Returns<string>(p => new Module(p));
            return factory.Object;
        }

        public void Dispose()
        {
            Directory.Delete(root, true);
        }
    }
}
