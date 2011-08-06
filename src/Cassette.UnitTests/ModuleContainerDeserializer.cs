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
    public class ModuleContainerDeserializer_Tests
    {
        [Fact]
        public void ReadingXElementCreatesModuleContainerWithModulesAndAssets()
        {
            var containerXml = new XDocument(new XElement("container",
                new XAttribute("rootDirectory", "c:\\test"),
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
                { "ScriptModules\\container.xml", containerXml.ToString().AsStream() },
                { "ScriptModules\\module-a", "module-a".AsStream() },
                { "ScriptModules\\module-b", "module-b".AsStream() }
            };
            Func<string, Stream> openFile = s => fileStreams[s];
            var moduleFactory = new Mock<IModuleFactory<ScriptModule>>();
            moduleFactory.Setup(f => f.CreateModule("c:\\test\\module-a")).Returns(new ScriptModule("c:\\test\\module-a"));
            moduleFactory.Setup(f => f.CreateModule("c:\\test\\module-b")).Returns(new ScriptModule("c:\\test\\module-b"));
            var deserializer = new ModuleContainerDeserializer<ScriptModule>(openFile, moduleFactory.Object);
            
            var container = deserializer.LoadModuleContainer();

            var moduleA = container.First(m => m.Directory.EndsWith("module-a"));
            moduleA.Assets.Count.ShouldEqual(1);
            moduleA.Assets[0].References.Single().ReferencedFilename.ShouldEqual("c:\\test\\module-b");
            moduleA.ContainsPath("c:\\test\\module-a\\asset-1.js");
            moduleA.ContainsPath("c:\\test\\module-a\\asset-2.js");
            moduleA.ContainsPath("c:\\test\\module-a");

            var moduleB = container.First(m => m.Directory.EndsWith("module-b"));
            moduleB.Assets.Count.ShouldEqual(1);
            moduleB.Assets[0].References.Count().ShouldEqual(0);
            moduleB.ContainsPath("c:\\test\\module-b\\asset-3.js");
            moduleB.ContainsPath("c:\\test\\module-b\\asset-4.js");
            moduleB.ContainsPath("c:\\test\\module-b");
        }
    }
}
