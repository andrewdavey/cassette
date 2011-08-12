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
    public class ModuleCache_IsUpToDate_Tests
    {
        public ModuleCache_IsUpToDate_Tests()
        {
            fileSystem = new Mock<IFileSystem>();
            cache = new ModuleCache<Module>(fileSystem.Object, Mock.Of<IModuleFactory<Module>>());
        }

        readonly ModuleCache<Module> cache;
        readonly Mock<IFileSystem> fileSystem;

        [Fact]
        public void WhenContainerFileDoesNotExist_ThenIsUpToDateReturnsFalse()
        {
            fileSystem.Setup(fs => fs.FileExists("container.xml"))
                      .Returns(false);
            cache.IsUpToDate(new DateTime(2000, 1, 2), "1.0.0.0").ShouldEqual(false);
        }

        [Fact]
        public void WhenContainerFileIsOlder_ThenIsUpToDateReturnsFalse()
        {
            fileSystem.Setup(fs => fs.FileExists("container.xml"))
                      .Returns(true);
            fileSystem.Setup(fs => fs.GetLastWriteTimeUtc("container.xml"))
                      .Returns(new DateTime(2000, 1, 1));
            fileSystem.Setup(fs => fs.FileExists("version"))
                      .Returns(true);
            fileSystem.Setup(fs => fs.OpenFile("version", FileMode.Open, FileAccess.Read))
                      .Returns("1.0.0.0".AsStream());

            cache.IsUpToDate(new DateTime(2000, 1, 2), "1.0.0.0").ShouldEqual(false);
        }

        [Fact]
        public void WhenContainerFileIsNewer_ThenIsUpToDateReturnsTrue()
        {
            fileSystem.Setup(fs => fs.FileExists("container.xml"))
                      .Returns(true);
            fileSystem.Setup(fs => fs.GetLastWriteTimeUtc("container.xml"))
                      .Returns(new DateTime(2000, 1, 2));
            fileSystem.Setup(fs => fs.FileExists("version"))
                      .Returns(true);
            fileSystem.Setup(fs => fs.OpenFile("version", FileMode.Open, FileAccess.Read))
                      .Returns("1.0.0.0".AsStream());

            cache.IsUpToDate(new DateTime(2000, 1, 1), "1.0.0.0").ShouldEqual(true);
        }

        [Fact]
        public void WhenContainerFileIsSameAge_ThenIsUpToDateReturnsTrue()
        {
            fileSystem.Setup(fs => fs.FileExists("container.xml"))
                      .Returns(true);
            fileSystem.Setup(fs => fs.GetLastWriteTimeUtc("container.xml"))
                      .Returns(new DateTime(2000, 1, 1));
            fileSystem.Setup(fs => fs.FileExists("version"))
                      .Returns(true);
            fileSystem.Setup(fs => fs.OpenFile("version", FileMode.Open, FileAccess.Read))
                      .Returns("1.0.0.0".AsStream());

            cache.IsUpToDate(new DateTime(2000, 1, 1), "1.0.0.0").ShouldEqual(true);
        }

        [Fact]
        public void WhenVersionNotEqual_ThenIsUpToDateReturnsFalse()
        {
            fileSystem.Setup(fs => fs.FileExists("version"))
                      .Returns(true);
            fileSystem.Setup(fs => fs.OpenFile("version", FileMode.Open, FileAccess.Read))
                      .Returns("1.0.0.0".AsStream());
            fileSystem.Setup(fs => fs.FileExists("container.xml"))
                      .Returns(true);
            fileSystem.Setup(fs => fs.GetLastWriteTimeUtc("container.xml"))
                      .Returns(new DateTime(2000, 1, 1));

            cache.IsUpToDate(new DateTime(2000, 1, 1), "2.0.0.0").ShouldEqual(false);
        }
    }

    public class ModuleCache_LoadModuleContainer_Tests
    {
        public ModuleCache_LoadModuleContainer_Tests()
        {
            var containerXml = new XDocument(new XElement("container",
                new XAttribute("lastWriteTime", DateTime.UtcNow.Ticks),
                new XElement("module",
                    new XAttribute("directory", "module-a"),
                    new XAttribute("hash", "010203"),
                    new XElement("asset",
                        new XAttribute("filename", "asset-1.js")
                    ),
                    new XElement("asset",
                        new XAttribute("filename", "asset-2.js")
                    ),
                    new XElement("reference", new XAttribute("path", "module-b"))
                ),
                new XElement("module",
                    new XAttribute("directory", "module-b"),
                    new XAttribute("hash", "0a0b0c"),
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

            var moduleFactory = new Mock<IModuleFactory<Module>>();
            moduleFactory.Setup(f => f.CreateModule(It.IsAny<string>()))
                         .Returns<string>(path => new Module(path));

            cache = new ModuleCache<Module>(fileSystem, moduleFactory.Object);
        }

        readonly ModuleCache<Module> cache;

        [Fact]
        public void LoadModuleContainer_ReturnsModuleContainer()
        {
            var container = cache.LoadModuleContainer();

            var moduleA = container.Modules.First(m => m.Directory.EndsWith("module-a"));
            moduleA.Assets.Count.ShouldEqual(1);
            moduleA.Assets[0].References.Single().ReferencedPath.ShouldEqual("module-b");
            moduleA.Assets[0].Hash.SequenceEqual(new byte[] { 1, 2, 3 }).ShouldBeTrue();
            moduleA.ContainsPath("module-a\\asset-1.js");
            moduleA.ContainsPath("module-a\\asset-2.js");
            moduleA.ContainsPath("module-a");

            var moduleB = container.Modules.First(m => m.Directory.EndsWith("module-b"));
            moduleB.Assets.Count.ShouldEqual(1);
            moduleB.Assets[0].Hash.SequenceEqual(new byte[] { 0xa, 0xb, 0xc }).ShouldBeTrue();
            moduleB.Assets[0].References.Count().ShouldEqual(0);
            moduleB.ContainsPath("module-b\\asset-3.js");
            moduleB.ContainsPath("module-b\\asset-4.js");
            moduleB.ContainsPath("module-b");
        }
    }

    public class ModuleCache_SaveModuleContainer_Tests
    {
        public ModuleCache_SaveModuleContainer_Tests()
        {
            fileSystem = new Mock<IFileSystem>();
            fileSystem.Setup(fs => fs.OpenFile(It.IsAny<string>(), FileMode.Create, FileAccess.Write))
                      .Returns(Stream.Null);

            cache = new ModuleCache<Module>(fileSystem.Object, Mock.Of<IModuleFactory<Module>>());
        }

        readonly ModuleCache<Module> cache;
        readonly Mock<IFileSystem> fileSystem;

        [Fact]
        public void SaveWritesContainerXmlFile()
        {
            var module = new Module("");
            var asset1 = new Mock<IAsset>();
            asset1.SetupGet(a => a.SourceFilename).Returns("asset.js");
            asset1.SetupGet(a => a.Hash).Returns(new byte[] { 1, 2, 3 });
            asset1.Setup(a => a.OpenStream()).Returns(Stream.Null);
            module.Assets.Add(asset1.Object);
            var container = new ModuleContainer<Module>(new[] { module });

            cache.SaveModuleContainer(container, "1.0.0.0");

            fileSystem.Verify(fs => fs.OpenFile("container.xml", FileMode.Create, FileAccess.Write));
            fileSystem.Verify(fs => fs.OpenFile("version", FileMode.Create, FileAccess.Write));
            fileSystem.Verify(fs => fs.OpenFile(".module", FileMode.Create, FileAccess.Write));
        }

        [Fact]
        public void ModuleHashIsSavedInContainerXml()
        {
            var module = new Module("test");
            var asset = new Mock<IAsset>();
            asset.SetupGet(a => a.SourceFilename).Returns("asset.js");
            asset.SetupGet(a => a.Hash).Returns(new byte[] { 1, 2, 3 });
            asset.Setup(a => a.OpenStream()).Returns(Stream.Null);
            module.Assets.Add(asset.Object);

            var temp = Path.GetTempFileName();
            try
            {
                fileSystem.Setup(fs => fs.OpenFile("container.xml", FileMode.Create, FileAccess.Write))
                          .Returns(() => File.OpenWrite(temp));

                var container = new ModuleContainer<Module>(new[] { module });
                cache.SaveModuleContainer(container, "1.0.0.0");

                var xml = File.ReadAllText(temp);
                xml.ShouldContain("<module directory=\"test\" hash=\"010203\" />");
            }
            finally
            {
                File.Delete(temp);
            }
        }

        [Fact]
        public void ModuleReferencesAreSavedInContainerXml()
        {
            var moduleA = new Module("module-a");
            var moduleB = new Module("module-b");

            var assetA = new Mock<IAsset>();
            assetA.SetupGet(a => a.References).Returns(new[] {
                new AssetReference("module-b", assetA.Object, 0, AssetReferenceType.DifferentModule)
            });
            assetA.Setup(a => a.OpenStream()).Returns(Stream.Null);
            moduleA.Assets.Add(assetA.Object);

            var assetB = new Mock<IAsset>();
            assetB.Setup(a => a.OpenStream()).Returns(Stream.Null);
            moduleB.Assets.Add(assetB.Object);

            var temp = Path.GetTempFileName();
            try
            {
                fileSystem.Setup(fs => fs.OpenFile("container.xml", FileMode.Create, FileAccess.Write))
                            .Returns(() => File.OpenWrite(temp));

                var container = new ModuleContainer<Module>(new[] { moduleA, moduleB });
                cache.SaveModuleContainer(container, "1.0.0.0");

                var xml = File.ReadAllText(temp);
                xml.ShouldContain("<reference path=\"module-b\" />");
            }
            finally
            {
                File.Delete(temp);
            }
        }
    }
}