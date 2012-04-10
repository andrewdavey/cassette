using System;
using System.IO;
using Cassette.Configuration;
using Cassette.IO;
using Cassette.Scripts;
using Cassette.Utilities;
using Moq;
using Should;
using Xunit;

namespace Cassette
{
    public class BundleCollection_AddExplicitFiles_Tests : IDisposable
    {
        readonly BundleCollection bundles;
        readonly TempDirectory temp;
        readonly CassetteSettings settings;

        public BundleCollection_AddExplicitFiles_Tests()
        {
            temp = new TempDirectory();
            settings = new CassetteSettings()
            {
                SourceDirectory = new FileSystemDirectory(temp)
            };
            bundles = new BundleCollection(settings, Mock.Of<IFileSearchProvider>(), Mock.Of<IBundleFactoryProvider>());
        }

        [Fact]
        public void AddWithExplicitFileCreatesBundleWithAsset()
        {
            File.WriteAllText(Path.Combine(temp, "file1.js"), "");

            bundles.Add<ScriptBundle>("~/path", new[] { "~/file1.js" });

            bundles["~/path"].Assets[0].Path.ShouldEqual("~/file1.js");
        }

        [Fact]
        public void AddWithExplicitFilesAsParamsArrayCreatesBundleWithAsset()
        {
            File.WriteAllText(Path.Combine(temp, "file1.js"), "");
            File.WriteAllText(Path.Combine(temp, "file2.js"), "");

            bundles.Add<ScriptBundle>("~/path", "~/file1.js", "~/file2.js");

            bundles["~/path"].Assets[0].Path.ShouldEqual("~/file1.js");
            bundles["~/path"].Assets[1].Path.ShouldEqual("~/file2.js");
        }

        [Fact]
        public void AddWithTwoExplicitFileCreatesBundleWithTwoAssets()
        {
            File.WriteAllText(Path.Combine(temp, "file1.js"), "");
            File.WriteAllText(Path.Combine(temp, "file2.js"), "");

            bundles.Add<ScriptBundle>("~/path", new[] { "~/file1.js", "~/file2.js" });

            bundles["~/path"].Assets[0].Path.ShouldEqual("~/file1.js");
            bundles["~/path"].Assets[1].Path.ShouldEqual("~/file2.js");
        }

        [Fact]
        public void AddWithExplicitFileCreatesBundleThatIsSorted()
        {
            File.WriteAllText(Path.Combine(temp, "file1.js"), "");

            bundles.Add<ScriptBundle>("~/path", new[] { "~/file1.js" });

            bundles["~/path"].IsSorted.ShouldBeTrue();
        }

        [Fact]
        public void WhenAddWithExplicitFileNotStartingWithTilde_ThenAssetFileIsApplicationRelative()
        {
            File.WriteAllText(Path.Combine(temp, "file1.js"), "");

            bundles.Add<ScriptBundle>("~/path", new[] { "file1.js" });

            bundles["~/path"].Assets[0].Path.ShouldEqual("~/file1.js");
        }

        [Fact]
        public void WhenAddWithExplicitFileNotStartingWithTildeButBundleDirectoryExists_ThenAssetFileIsBundleRelative()
        {
            Directory.CreateDirectory(Path.Combine(temp, "bundle"));
            File.WriteAllText(PathUtilities.Combine(temp, "bundle", "file1.js"), "");

            bundles.Add<ScriptBundle>("~/bundle", new[] { "file1.js" });

            bundles["~/bundle"].Assets[0].Path.ShouldEqual("~/bundle/file1.js");
        }

        [Fact]
        public void WhenWithExplicitFileAndCustomizeAction_ThenCreatedBundleIsCustomized()
        {
            File.WriteAllText(Path.Combine(temp, "file1.js"), "");

            bundles.Add<ScriptBundle>("~/path", new[] { "~/file1.js" }, b => b.PageLocation = "test");

            bundles["~/path"].PageLocation.ShouldEqual("test");
        }

        public void Dispose()
        {
            temp.Dispose();
        }
    }
}