using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Xml.Linq;
using Cassette.Utilities;
using Moq;
using Should;
using Xunit;

namespace Cassette.Persistence
{
    public class ModuleContainerWriter_Tests
    {
        [Fact]
        public void SaveWritesContainerXmlFile()
        {
            var fileSystem = new Mock<IFileSystem>();
            fileSystem.Setup(fs => fs.OpenWrite(It.IsAny<string>())).Returns(Stream.Null);

            var now = DateTime.UtcNow;
            var modules = new[] {
                new ScriptModule("", _ => null)
            };
            var asset1 = new Mock<IAsset>();
            asset1.SetupGet(a => a.SourceFilename).Returns("asset.js");
            asset1.Setup(a => a.OpenStream()).Returns(Stream.Null);
            modules[0].Assets.Add(asset1.Object);
            var container = new ModuleContainer<ScriptModule>(modules, now);

            var writer = new ModuleContainerWriter<ScriptModule>(fileSystem.Object);
            writer.Save(container);

            fileSystem.Verify(fs => fs.OpenWrite("container.xml"));
            fileSystem.Verify(fs => fs.OpenWrite(".module"));
        }
    }
}
