using System;
using System.IO;
using System.Linq;
using Moq;
using Xunit;

namespace Cassette.Persistence
{
    public class ModuleContainerWriter_Tests
    {
        public ModuleContainerWriter_Tests()
        {
            fileSystem = new Mock<IFileSystem>();
            fileSystem.Setup(fs => fs.OpenFile(It.IsAny<string>(), FileMode.OpenOrCreate, FileAccess.Write)).Returns(Stream.Null);
        }

        readonly Mock<IFileSystem> fileSystem;

        [Fact]
        public void SaveWritesContainerXmlFile()
        {
            var modules = new[] {
                new ScriptModule("", Mock.Of<IFileSystem>())
            };
            var asset1 = new Mock<IAsset>();
            asset1.SetupGet(a => a.SourceFilename).Returns("asset.js");
            asset1.Setup(a => a.OpenStream()).Returns(Stream.Null);
            modules[0].Assets.Add(asset1.Object);
            var container = new ModuleContainer<ScriptModule>(modules);

            var writer = new ModuleContainerWriter<ScriptModule>(fileSystem.Object);
            writer.Save(container);

            fileSystem.Verify(fs => fs.OpenFile("container.xml", FileMode.OpenOrCreate, FileAccess.Write));
            fileSystem.Verify(fs => fs.OpenFile(".module", FileMode.OpenOrCreate, FileAccess.Write));
        }

        [Fact]
        public void SaveDeletesAllCurrentFileSystemContent()
        {
            var writer = new ModuleContainerWriter<ScriptModule>(fileSystem.Object);
            writer.Save(new ModuleContainer<ScriptModule>(Enumerable.Empty<ScriptModule>()));
            fileSystem.Verify(fs => fs.DeleteAll());
        }
    }
}
