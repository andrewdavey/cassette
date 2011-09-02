using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Cassette.IO;
using Should;
using Xunit;
using Moq;
using Cassette.Scripts;
using System.IO;

namespace Cassette
{
    class FileSystemModuleSource_Tests : IDisposable
    {
        DirectoryInfo root;
        Mock<ICassetteApplication> application;

        class TestableFileSystemModuleSource : FileSystemModuleSource<Module>
        {
            protected override IEnumerable<string> GetModuleDirectoryPaths(ICassetteApplication application)
            {
                yield return "~/test";
            }
        }

        public FileSystemModuleSource_Tests()
        {
            root = Directory.CreateDirectory(Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString()));
            root.CreateSubdirectory("test");
            application = new Mock<ICassetteApplication>();
            application
                .Setup(a => a.RootDirectory)
                .Returns(new FileSystemDirectory(root.FullName));
        }

        [Fact]
        public void GivenModuleDescriptorWithExternalUrl_WhenGetModules_ThenResultContainsExternalModule()
        {
            File.WriteAllText(
                Path.Combine(root.FullName, "test", "module.txt"),
                "[external]\nurl = http://test.com"
            );

            var factory = new Mock<IModuleFactory<Module>>();
            factory
                .Setup(f => f.CreateExternalModule("~/test", It.IsAny<ModuleDescriptor>()))
                .Returns(new ExternalScriptModule("~/test", "http://test.com"));

            var source = new TestableFileSystemModuleSource();
            var modules = source.GetModules(factory.Object, application.Object).ToArray();

            modules[0].ShouldBeType<ExternalScriptModule>();
        }

        public void Dispose()
        {
            root.Delete(true);
        }
    }
}
