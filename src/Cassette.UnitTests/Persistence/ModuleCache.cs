using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Xml.Linq;
using Cassette.ModuleProcessing;
using Cassette.Utilities;
using Moq;
using Should;
using Xunit;

namespace Cassette.Persistence
{
    public class ModuleCache_IsUpToDate_Tests
    {
        public ModuleCache_IsUpToDate_Tests()
        {
            sourceFileSystem = new Mock<IFileSystem>();
            cacheFileSystem = new Mock<IFileSystem>();
            cache = new ModuleCache<Module>(cacheFileSystem.Object, Mock.Of<IModuleFactory<Module>>());

            // Stub the container XML file content.
            cacheFileSystem.Setup(fs => fs.OpenFile("container.xml", FileMode.Open, FileAccess.Read))
                      .Returns(() => @"<?xml version=""1.0""?>
<container>
    <module directory="""" hash=""""/>
</container>".AsStream());
        }

        readonly ModuleCache<Module> cache;
        readonly Mock<IFileSystem> cacheFileSystem;
        readonly Mock<IFileSystem> sourceFileSystem;

        [Fact]
        public void WhenContainerFileDoesNotExist_ThenIsUpToDateReturnsFalse()
        {
            cacheFileSystem.Setup(fs => fs.FileExists("container.xml"))
                      .Returns(false);
            cache.IsUpToDate(new DateTime(2000, 1, 2), "1.0.0.0", sourceFileSystem.Object).ShouldEqual(false);
        }

        [Fact]
        public void WhenContainerFileIsOlder_ThenIsUpToDateReturnsFalse()
        {
            cacheFileSystem.Setup(fs => fs.FileExists("container.xml"))
                      .Returns(true);
            cacheFileSystem.Setup(fs => fs.GetLastWriteTimeUtc("container.xml"))
                      .Returns(new DateTime(2000, 1, 1));
            cacheFileSystem.Setup(fs => fs.FileExists("version"))
                      .Returns(true);
            cacheFileSystem.Setup(fs => fs.OpenFile("version", FileMode.Open, FileAccess.Read))
                      .Returns("1.0.0.0".AsStream());

            cache.IsUpToDate(new DateTime(2000, 1, 2), "1.0.0.0", sourceFileSystem.Object).ShouldEqual(false);
        }

        [Fact]
        public void WhenContainerFileIsNewer_ThenIsUpToDateReturnsTrue()
        {
            cacheFileSystem.Setup(fs => fs.FileExists("container.xml"))
                      .Returns(true);
            cacheFileSystem.Setup(fs => fs.GetLastWriteTimeUtc("container.xml"))
                      .Returns(new DateTime(2000, 1, 2));
            cacheFileSystem.Setup(fs => fs.FileExists("version"))
                      .Returns(true);
            cacheFileSystem.Setup(fs => fs.OpenFile("version", FileMode.Open, FileAccess.Read))
                      .Returns("1.0.0.0".AsStream());

            cache.IsUpToDate(new DateTime(2000, 1, 1), "1.0.0.0", sourceFileSystem.Object).ShouldEqual(true);
        }

        [Fact]
        public void WhenContainerFileIsSameAge_ThenIsUpToDateReturnsTrue()
        {
            cacheFileSystem.Setup(fs => fs.FileExists("container.xml"))
                      .Returns(true);
            cacheFileSystem.Setup(fs => fs.GetLastWriteTimeUtc("container.xml"))
                      .Returns(new DateTime(2000, 1, 1));
            cacheFileSystem.Setup(fs => fs.FileExists("version"))
                      .Returns(true);
            cacheFileSystem.Setup(fs => fs.OpenFile("version", FileMode.Open, FileAccess.Read))
                      .Returns("1.0.0.0".AsStream());

            cache.IsUpToDate(new DateTime(2000, 1, 1), "1.0.0.0", sourceFileSystem.Object).ShouldEqual(true);
        }

        [Fact]
        public void WhenVersionNotEqual_ThenIsUpToDateReturnsFalse()
        {
            cacheFileSystem.Setup(fs => fs.FileExists("version"))
                      .Returns(true);
            cacheFileSystem.Setup(fs => fs.OpenFile("version", FileMode.Open, FileAccess.Read))
                      .Returns("1.0.0.0".AsStream());
            cacheFileSystem.Setup(fs => fs.FileExists("container.xml"))
                      .Returns(true);
            cacheFileSystem.Setup(fs => fs.GetLastWriteTimeUtc("container.xml"))
                      .Returns(new DateTime(2000, 1, 1));

            cache.IsUpToDate(new DateTime(2000, 1, 1), "2.0.0.0", sourceFileSystem.Object).ShouldEqual(false);
        }

        [Fact]
        public void GivenPreviouslyUsedAssetNoLongerExists_ThenIsUpToDateReturnsFalse()
        {
            var cacheDirectory = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString());
            Directory.CreateDirectory(cacheDirectory);
            var sourceDirectory = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString());
            Directory.CreateDirectory(sourceDirectory);
            try
            {
                File.WriteAllText(Path.Combine(cacheDirectory, "version"), "1.0");
                File.WriteAllText(Path.Combine(cacheDirectory, "container.xml"), 
                    @"<?xml version=""1.0""?>
<container>
    <module directory="""" hash="""">
        <asset filename=""test.js""/>
    </module>
</container>");
                var cache = new ModuleCache<Module>(new FileSystem(cacheDirectory), Mock.Of<IModuleFactory<Module>>());
                cache.IsUpToDate(DateTime.UtcNow.AddDays(-1), "1.0", new FileSystem(sourceDirectory)).ShouldBeFalse();
            }
            finally
            {
                Directory.Delete(cacheDirectory, true);
                Directory.Delete(sourceDirectory);
            }
        }

        [Fact]
        public void GivenRawFileChangedSinceCacheCreated_ThenIsUpToDateReturnsFalse()
        {
            var cacheDirectory = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString());
            Directory.CreateDirectory(cacheDirectory);
            var sourceDirectory = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString());
            Directory.CreateDirectory(sourceDirectory);
            try
            {
                File.WriteAllText(Path.Combine(cacheDirectory, "version"), "1.0");
                File.WriteAllText(Path.Combine(cacheDirectory, "container.xml"),
                    @"<?xml version=""1.0""?>
<container>
    <module directory="""" hash="""">
        <rawFileReference filename=""test.png""/>
    </module>
</container>");
                File.WriteAllText(Path.Combine(sourceDirectory, "test.png"), "");
                var cache = new ModuleCache<Module>(new FileSystem(cacheDirectory), Mock.Of<IModuleFactory<Module>>());
                cache.IsUpToDate(DateTime.UtcNow.AddDays(-1), "1.0", new FileSystem(sourceDirectory)).ShouldBeFalse();
            }
            finally
            {
                Directory.Delete(cacheDirectory, true);
                Directory.Delete(sourceDirectory, true);
            }
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
            var modules = cache.LoadModules();

            var moduleA = modules.First(m => m.Path.EndsWith("module-a"));
            moduleA.Assets.Count.ShouldEqual(1);
            moduleA.Assets[0].References.Single().ReferencedPath.ShouldEqual("module-b");
            moduleA.Assets[0].Hash.SequenceEqual(new byte[] { 1, 2, 3 }).ShouldBeTrue();
            moduleA.ContainsPath("module-a\\asset-1.js");
            moduleA.ContainsPath("module-a\\asset-2.js");
            moduleA.ContainsPath("module-a");

            var moduleB = modules.First(m => m.Path.EndsWith("module-b"));
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

        [Fact]
        public void DuplicateModuleReferencesAreOnlyWrittenOnce()
        {
            var moduleA = new Module("module-a");
            var moduleB = new Module("module-b");

            var assetA = new Mock<IAsset>();
            assetA.SetupGet(a => a.References).Returns(new[] {
                new AssetReference("module-b\\1.js", assetA.Object, 0, AssetReferenceType.DifferentModule),
                new AssetReference("module-b\\2.js", assetA.Object, 0, AssetReferenceType.DifferentModule)
            });
            assetA.Setup(a => a.OpenStream()).Returns(Stream.Null);
            moduleA.Assets.Add(assetA.Object);

            var assetB1 = StubAsset("1.js");
            var assetB2 = StubAsset("2.js");
            var assetB = new ConcatenatedAsset(new[] { assetB1.Object, assetB2.Object });
            moduleB.Assets.Add(assetB);

            var temp = Path.GetTempFileName();
            try
            {
                fileSystem.Setup(fs => fs.OpenFile("container.xml", FileMode.Create, FileAccess.Write))
                          .Returns(() => File.OpenWrite(temp));

                var container = new ModuleContainer<Module>(new[] { moduleA, moduleB });
                cache.SaveModuleContainer(container, "1.0.0.0");

                var xml = File.ReadAllText(temp);
                Regex.Matches(xml, Regex.Escape("<reference path=\"module-b\" />")).Count.ShouldEqual(1);
            }
            finally
            {
                File.Delete(temp);
            }
        }

        Mock<IAsset> StubAsset(string filename)
        {
            var asset = new Mock<IAsset>();
            asset.Setup(a => a.Accept(It.IsAny<IAssetVisitor>()))
                 .Callback<IAssetVisitor>(v => v.Visit(asset.Object));
            asset.SetupGet(a => a.SourceFilename)
                 .Returns(filename);
            asset.Setup(c => c.OpenStream())
                 .Returns(Stream.Null);
            return asset;
        }
    }
}