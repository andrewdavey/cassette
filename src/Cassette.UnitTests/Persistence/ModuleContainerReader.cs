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
    public class ModuleContainerReader_Tests
    {
        [Fact]
        public void ReadingXElementCreatesModuleContainerWithModulesAndAssets()
        {
            var containerXml = new XDocument(new XElement("container",
                new XAttribute("lastWriteTime", DateTime.UtcNow.Ticks),
                new XElement("module",
                    new XAttribute("directory", "module-a"),
                    new XElement("asset",
                        new XAttribute("filename", "asset-1.js")
                    ),
                    new XElement("asset",
                        new XAttribute("filename", "asset-2.js")
                    ),
                    new XElement("reference", new XAttribute("module", "module-b"))
                ),
                new XElement("module",
                    new XAttribute("directory", "module-b"),
                    new XElement("asset",
                        new XAttribute("filename", "asset-3.js")
                    ),
                    new XElement("asset",
                        new XAttribute("filename", "asset-4.js")
                    )
                )
            ));
            var fileStreams = new Dictionary<string, Stream>
            {
                { "container.xml", containerXml.ToString().AsStream() },
                { "module-a", "module-a".AsStream() },
                { "module-b", "module-b".AsStream() }
            };
            var fileSystem = new StubFileSystem(fileStreams);
            var moduleFactory = new Mock<IModuleFactory<ScriptModule>>();
            moduleFactory.Setup(f => f.CreateModule("module-a")).Returns(new ScriptModule("module-a", _ => null));
            moduleFactory.Setup(f => f.CreateModule("module-b")).Returns(new ScriptModule("module-b", _ => null));
            var reader = new ModuleContainerReader<ScriptModule>(fileSystem, moduleFactory.Object);

            var container = reader.Load();

            var moduleA = container.First(m => m.Directory.EndsWith("module-a"));
            moduleA.Assets.Count.ShouldEqual(1);
            moduleA.Assets[0].References.Single().ReferencedFilename.ShouldEqual("module-b");
            moduleA.ContainsPath("module-a\\asset-1.js");
            moduleA.ContainsPath("module-a\\asset-2.js");
            moduleA.ContainsPath("module-a");

            var moduleB = container.First(m => m.Directory.EndsWith("module-b"));
            moduleB.Assets.Count.ShouldEqual(1);
            moduleB.Assets[0].References.Count().ShouldEqual(0);
            moduleB.ContainsPath("module-b\\asset-3.js");
            moduleB.ContainsPath("module-b\\asset-4.js");
            moduleB.ContainsPath("module-b");
        }

        [Fact]
        public void GivenNoFilesExist_LoadReturnsEmptyModuleContainer()
        {
            var reader = new ModuleContainerReader<ScriptModule>(Mock.Of<IFileSystem>(), Mock.Of<IModuleFactory<ScriptModule>>());
            var container = reader.Load();

            container.ShouldBeEmpty();
        }

        [Fact]
        public void GivenNoFilesExist_LoadReturnsModuleContainerWithMinLastWriteDate()
        {
            var reader = new ModuleContainerReader<ScriptModule>(Mock.Of<IFileSystem>(), Mock.Of<IModuleFactory<ScriptModule>>());
            var container = reader.Load();

            container.LastWriteTime.ShouldEqual(DateTime.MinValue);
        }

    }
}
