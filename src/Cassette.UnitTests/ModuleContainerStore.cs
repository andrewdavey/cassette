using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Xml.Linq;
using Cassette.Utilities;
using Moq;
using Should;
using Xunit;

namespace Cassette
{
    public class ModuleContainerStore_Tests
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
            var store = new ModuleContainerStore<ScriptModule>(fileSystem, moduleFactory.Object);
            
            var container = store.Load();

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
            var store = new ModuleContainerStore<ScriptModule>(Mock.Of<IFileSystem>(), Mock.Of<IModuleFactory<ScriptModule>>());
            var container = store.Load();

            container.ShouldBeEmpty();
        }

        [Fact]
        public void GivenNoFilesExist_LoadReturnsModuleContainerWithMinLastWriteDate()
        {
            var store = new ModuleContainerStore<ScriptModule>(Mock.Of<IFileSystem>(), Mock.Of<IModuleFactory<ScriptModule>>());
            var container = store.Load();

            container.LastWriteTime.ShouldEqual(DateTime.MinValue);
        }

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
            var container = new ModuleContainer<ScriptModule>(modules, now, "c:\\test");

            var store = new ModuleContainerStore<ScriptModule>(fileSystem.Object, Mock.Of<IModuleFactory<ScriptModule>>());
            store.Save(container);

            fileSystem.Verify(fs => fs.OpenWrite("container.xml"));
            fileSystem.Verify(fs => fs.OpenWrite("_.module"));
        }
    }
}
