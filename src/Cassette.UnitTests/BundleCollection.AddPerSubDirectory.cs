using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Cassette.IO;
using Cassette.Utilities;
using Moq;
using Should;
using Xunit;
using Cassette.Scripts;
using Cassette.BundleProcessing;

namespace Cassette
{
    public class BundleCollection_AddPerSubDirectory_Tests : BundleCollectionTestsBase
    {
        TestableBundle createdBundle;

        public BundleCollection_AddPerSubDirectory_Tests()
        {
            factory
                .Setup(f => f.CreateBundle(It.IsAny<string>(), It.IsAny<IEnumerable<IFile>>(), It.IsAny<BundleDescriptor>()))
                .Returns<string, IEnumerable<IFile>, BundleDescriptor>(
                    (path, files, d) => createdBundle = new TestableBundle(path)
                );
        }

        [Fact]
        public void GivenTwoSubDirectoriesWithFiles_WhenAddPerSubDirectory_ThenTwoBundlesAreAdded()
        {
            settings.SourceDirectory = new FakeFileSystem
            {
                "~/bundle-a/file.js",
                "~/bundle-b/file.js",
            };

            bundles.AddPerSubDirectory<TestableBundle>("~");

            bundles["~/bundle-a"].ShouldBeType<TestableBundle>();
            bundles["~/bundle-b"].ShouldBeType<TestableBundle>();
        }

        [Fact]
        public void GivenCustomFileSearch_WhenAddPerSubDirectory_ThenFileSearchIsUsedToGetAssets()
        {
            settings.SourceDirectory = new FakeFileSystem
            {
                "~/bundle/file.js"
            };
            fileSearch.Setup(s => s.FindFiles(It.IsAny<IDirectory>()))
                .Returns(new[] { settings.SourceDirectory.GetFile("~/bundle/file.js") })
                .Verifiable();
            CreateDirectory("bundle");

            bundles.AddPerSubDirectory<TestableBundle>("~", fileSearch.Object);

            fileSearch.Verify();
        }

        [Fact]
        public void GivenBundleCustomizeAction_WhenAddPerSubDirectory_ThenActionIsCalledWithBundle()
        {
            settings.SourceDirectory = new FakeFileSystem
            {
                "~/bundle/file.js"
            };

            Bundle bundle = null;
            bundles.AddPerSubDirectory<TestableBundle>("~", b => bundle = b);

            bundle.ShouldBeSameAs(createdBundle);
        }
        
        [Fact]
        public void GivenHiddenDirectory_WhenAddPerSubDirectory_ThenDirectoryIsIgnored()
        {
            CreateDirectory("test");
            File.SetAttributes(Path.Combine(tempDirectory, "test"), FileAttributes.Directory | FileAttributes.Hidden);

            bundles.AddPerSubDirectory<TestableBundle>("~");

            bundles.ShouldBeEmpty();
        }

        [Fact]
        public void GivenEmptyDirectory_WhenAddPerSubDirectory_ThenDirectoryIsIgnored()
        {
            CreateDirectory("test");

            bundles.AddPerSubDirectory<TestableBundle>("~");

            bundles.ShouldBeEmpty();
        }

        [Fact]
        public void GivenDirectoryWithExternalBundleDescriptorButNoAssets_WhenAddPerSubDirectory_ThenBundleCreatedForDirectory()
        {
            CreateDirectory("test");
            File.WriteAllText(
                PathUtilities.Combine(tempDirectory, "test", "bundle.txt"), 
                "[external]" + Environment.NewLine + "url=http://example.org/"
                );
            bundles.AddPerSubDirectory<TestableBundle>("~");
            bundles.Count().ShouldEqual(1);
        }

        [Fact]
        public void GivenTopLevelDirectoryWithExternalBundleDescriptorButNoAssets_WhenAddPerSubDirectory_ThenBundleCreatedForDirectory()
        {
            var bundle = new Mock<TestableBundle>("~");
            bundle.As<IExternalBundle>();
            factory.Setup(f => f.CreateBundle(It.IsAny<string>(), It.IsAny<IEnumerable<IFile>>(), It.IsAny<BundleDescriptor>()))
                .Returns<string, IEnumerable<IFile>, BundleDescriptor>(
                    (path, files, d) => createdBundle = bundle.Object
                );

            File.WriteAllText(
                Path.Combine(tempDirectory, "bundle.txt"),
                "[external]" + Environment.NewLine + "url=http://example.org/"
                );
            bundles.AddPerSubDirectory<TestableBundle>("~");
            bundles.Count().ShouldEqual(1);
        }

        [Fact]
        public void GivenTopLevelDirectoryHasFilesAndSubDirectory_WhenAddPerSubDirectory_ThenBundleAlsoCreatedForTopLevel()
        {
            settings.SourceDirectory = new FakeFileSystem
            {
                "~/file-a.js",
                "~/test/file-b.js"
            };

            bundles.AddPerSubDirectory<TestableBundle>("~");

            bundles.Count().ShouldEqual(2);

            factory.Verify(f => f.CreateBundle(
                "~",
                It.Is<IEnumerable<IFile>>(files => files.Count() == 1),
                It.IsAny<BundleDescriptor>())
                );
            factory.Verify(f => f.CreateBundle(
                "~/test",
                It.Is<IEnumerable<IFile>>(files => files.Count() == 1),
                It.IsAny<BundleDescriptor>())
                );
        }

        [Fact]
        public void GivenTopLevelDirectoryHasFilesAndSubDirectory_WhenAddPerSubDirectoryWithCustomizeAction_ThenBundleForTopLevelIsCustomized()
        {
            settings.SourceDirectory = new FakeFileSystem
            {
                "~/file-a.js",
                "~/test/file-b.js"
            };

            factory.Setup(f => f.CreateBundle(
                "~",
                It.Is<IEnumerable<IFile>>(files => files.Count() == 1),
                It.IsAny<BundleDescriptor>())
                ).Returns(new TestableBundle("~"));

            bundles.AddPerSubDirectory<TestableBundle>("~", b => b.PageLocation = "test");

            bundles["~"].PageLocation.ShouldEqual("test");
        }

        [Fact]
        public void GivenTopLevelDirectoryHasFilesAndSubDirectory_WhenAddPerSubDirectoryWithExcludeTopLevelTrue_ThenBundleNotCreatedForTopLevel()
        {
            settings.SourceDirectory = new FakeFileSystem
            {
                "~/file-a.js",
                "~/test/file-b.js"
            };

            bundles.AddPerSubDirectory<TestableBundle>("~", excludeTopLevel: true);

            bundles.Count().ShouldEqual(1);
            bundles["~/test"].ShouldBeType<TestableBundle>();
        }

        [Fact]
        public void GivenDirectoryWithExternalBundleDescriptorReferencingOutsideDirectory_WhenAddPerSubDirectory_ThenCreateWorks()
        {
            bundleFactoryProvider
                .Setup(f => f.GetBundleFactory<ScriptBundle>())
                .Returns(new ScriptBundleFactory(() => Mock.Of<IBundlePipeline<ScriptBundle>>()));

            CreateDirectory("test\\thebundle");

            File.WriteAllText(
                PathUtilities.Combine(tempDirectory, "test", "thebundle", "bundle.txt"),
                "[external]" + Environment.NewLine + "url=http://example.org/test.js" + Environment.NewLine + "[assets]" + Environment.NewLine + "~/test/test.js"
                );
            File.WriteAllText(Path.Combine(tempDirectory, "test", "test.js"), "");

            bundles.AddPerSubDirectory<ScriptBundle>("test");
            ScriptBundle bundle = bundles.Get<ScriptBundle>("test/thebundle");
            bundle.Assets.Count().ShouldEqual(1);
            bundle.Assets[0].Path.ShouldEqual("~/test/test.js");
        }

    }
}