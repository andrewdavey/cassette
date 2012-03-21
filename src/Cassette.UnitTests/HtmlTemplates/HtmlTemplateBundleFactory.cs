using System.Collections.Generic;
using System.Linq;
using Cassette.Configuration;
using Moq;
using Should;
using Xunit;
using Cassette.IO;

namespace Cassette.HtmlTemplates
{
    public class HtmlTemplateBundleFactory_Tests
    {
        readonly HtmlTemplateBundleFactory factory;
        readonly List<IFile> allFiles = new List<IFile>();
        readonly CassetteSettings settings;

        public HtmlTemplateBundleFactory_Tests()
        {
            settings = new CassetteSettings("");
            factory = new HtmlTemplateBundleFactory(settings);
        }

        void FilesExist(params string[] paths)
        {
            foreach (var path in paths)
            {
                var file = new Mock<IFile>();
                file.SetupGet(f => f.FullPath).Returns(path);
                allFiles.Add(file.Object);
            }
        }

        [Fact]
        public void CreateBundle_ReturnsHtmlTemplateBundleWithPathSet()
        {
            var bundle = factory.CreateBundle(
                "~/test",
                allFiles,
                new BundleDescriptor { AssetFilenames = { "*" } }
            );
            bundle.Path.ShouldEqual("~/test");
        }

        [Fact]
        public void GivenBundleDescriptorWithOnlyWildcardFilename_WhenCreateBundle_ThenReturnBundleWithAllAssets()
        {
            FilesExist("~/test/file.htm");
            var bundleDescriptor = new BundleDescriptor
            {
                AssetFilenames = { "*" }
            };

            var bundle = factory.CreateBundle("~/test", allFiles, bundleDescriptor);

            bundle.Assets[0].Path.ShouldEqual("~/test/file.htm");
        }

        [Fact]
        public void GivenBundleDescriptorWithOnlyExplicitFilename_WhenCreateBundle_ThenReturnBundleWithOnlySpecifiedAssets()
        {
            FilesExist("~/test/yes.htm", "~/test/no.htm");
            var bundleDescriptor = new BundleDescriptor
            {
                AssetFilenames = { "~/test/yes.htm" }
            };

            var bundle = factory.CreateBundle("~/test", allFiles, bundleDescriptor);

            bundle.Assets[0].Path.ShouldEqual("~/test/yes.htm");
            bundle.Assets.Count.ShouldEqual(1);
        }

        [Fact]
        public void GivenBundleDescriptorWithExplicitFilenamesThenWildcard_WhenCreateBundle_ThenReturnBundleWithSpecifiedAssetsThenAllTheRemainingAssets()
        {
            FilesExist(Enumerable.Range(0, 4).Select(i => "~/test/" + i + ".htm").ToArray());

            var bundleDescriptor = new BundleDescriptor
            {
                AssetFilenames = { "~/test/3.htm", "~/test/1.htm", "*" }
            };

            var bundle = factory.CreateBundle("~/test", allFiles, bundleDescriptor);

            bundle.Assets.Count.ShouldEqual(4);
            bundle.Assets[0].Path.ShouldEqual("~/test/3.htm");
            bundle.Assets[1].Path.ShouldEqual("~/test/1.htm");
            new HashSet<string>(bundle.Assets.Skip(2).Select(a => a.Path))
                .SetEquals(new[] { "~/test/0.htm", "~/test/2.htm" })
                .ShouldBeTrue();
        }

        [Fact]
        public void GivenBundleDescriptorWithExplicitFilenameHavingDifferentCasing_WhenCreateBundle_ThenBundleHasAssetForFile()
        {
            FilesExist("~/test/file.htm");
            var bundleDescriptor = new BundleDescriptor
            {
                AssetFilenames = { "~/test/FILE.HTM" }
            };

            var bundle = factory.CreateBundle("~/test", allFiles, bundleDescriptor);

            bundle.Assets[0].Path.ShouldEqual("~/test/file.htm");
        }

        [Fact]
        public void GivenBundleDescriptorWithExplicitFilename_WhenCreateBundle_ThenBundleIsSorted()
        {
            FilesExist("~/test/file.htm");
            var bundleDescriptor = new BundleDescriptor
            {
                AssetFilenames = { "~/test/file.htm" }
            };

            var bundle = factory.CreateBundle("~/test", allFiles, bundleDescriptor);

            bundle.IsSorted.ShouldBeTrue();
        }

        [Fact]
        public void GivenBundleDescriptorWithOnlyWildcard_WhenCreateBundle_ThenBundleIsSortedIsFalse()
        {
            FilesExist("~/test/file.htm");
            var bundleDescriptor = new BundleDescriptor
            {
                AssetFilenames = { "*" }
            };

            var bundle = factory.CreateBundle("~/test", allFiles, bundleDescriptor);

            bundle.IsSorted.ShouldBeFalse();
        }

        [Fact]
        public void GivenBundleDescriptorWithNoFilenames_WhenCreateBundle_ThenBundleIsSorted()
        {
            var bundleDescriptor = new BundleDescriptor();

            var bundle = factory.CreateBundle("~/test", new IFile[0], bundleDescriptor);

            bundle.IsSorted.ShouldBeTrue();
        }

        [Fact]
        public void GivenSubDirectoryAsterisks_WhenCreateBundle_ThenFilesFromSubDirectoriesAreIncluded()
        {
            // Thanks to maniserowicz for this idea

            FilesExist("~/shared/shared-test1.htm", "~/shared/shared-test2.htm", "~/app/app-test1.htm", "~/app/app-test2.htm");
            var bundleDescriptor = new BundleDescriptor
            {
                AssetFilenames = { "~/shared/*", "~/app/*" }
            };

            var bundle = factory.CreateBundle("~", allFiles, bundleDescriptor);
            
            bundle.Assets.Select(a => a.Path)
                .SequenceEqual(new[] { "~/shared/shared-test1.htm", "~/shared/shared-test2.htm", "~/app/app-test1.htm", "~/app/app-test2.htm" })
                .ShouldBeTrue();
        }

        [Fact]
        public void GivenExplicitSubDirFileAndThenSubDirAsterisk_WhenCreateBundle_ThenExplicitFileNotAddedTwice()
        {
            // Thanks to maniserowicz for this idea

            FilesExist("~/shared/shared-test1.htm", "~/shared/shared-test2.htm", "~/app/app-test1.htm", "~/app/app-test2.htm");
            var bundleDescriptor = new BundleDescriptor
            {
                AssetFilenames = { "~/shared/shared-test2.htm", "~/shared/*", "~/app/*" }
            };

            var bundle = factory.CreateBundle("~", allFiles, bundleDescriptor);

            bundle.Assets.Select(a => a.Path)
                .SequenceEqual(new[] { "~/shared/shared-test2.htm", "~/shared/shared-test1.htm", "~/app/app-test1.htm", "~/app/app-test2.htm" })
                .ShouldBeTrue();
        }

        [Fact]
        public void GivenSubDirAsteriskAndTopLevelAsterisk_WhenCreateBundle_ThenSubDirFilesNotAddedTwice()
        {
            FilesExist("~/shared/a.htm", "~/shared/b.htm", "~/c.htm");
            var bundleDescriptor = new BundleDescriptor
            {
                AssetFilenames = { "~/shared/*", "*" }
            };

            var bundle = factory.CreateBundle("~", allFiles, bundleDescriptor);

            bundle.Assets.Select(a => a.Path)
                .SequenceEqual(new[] { "~/shared/a.htm", "~/shared/b.htm", "~/c.htm" })
                .ShouldBeTrue();
        }

        [Fact]
        public void CreateBundleAssignsSettingsDefaultProcessor()
        {
            var processor = new HtmlTemplatePipeline();
            settings.ModifyDefaults<HtmlTemplateBundle>(defaults => defaults.BundlePipeline = processor);
            var bundle = factory.CreateBundle("~", Enumerable.Empty<IFile>(), new BundleDescriptor { AssetFilenames = { "*" } });
            bundle.Processor.ShouldBeSameAs(processor);
        }
    }
}