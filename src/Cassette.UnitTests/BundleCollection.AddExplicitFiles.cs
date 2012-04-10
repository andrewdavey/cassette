using System.IO;
using System.Linq;
using Cassette.Utilities;
using Should;
using Xunit;

namespace Cassette
{
    public class BundleCollection_AddExplicitFiles_Tests : BundleCollectionTestsBase
    {
        [Fact]
        public void AddWithExplicitFileCreatesBundleWithAsset()
        {
            File.WriteAllText(Path.Combine(tempDirectory, "file1.js"), "");

            bundles.Add<TestableBundle>("~/path", new[] { "~/file1.js" });

            FilesUsedToCreateBundle[0].ShouldEqual("~/file1.js");
        }

        [Fact]
        public void AddWithExplicitFilesAsParamsArrayCreatesBundleWithAsset()
        {
            File.WriteAllText(Path.Combine(tempDirectory, "file1.js"), "");
            File.WriteAllText(Path.Combine(tempDirectory, "file2.js"), "");

            bundles.Add<TestableBundle>("~/path", "~/file1.js", "~/file2.js");

            FilesUsedToCreateBundle.ShouldEqual(new[] { "~/file1.js", "~/file2.js" });
        }

        [Fact]
        public void AddWithTwoExplicitFileCreatesBundleWithTwoAssets()
        {
            File.WriteAllText(Path.Combine(tempDirectory, "file1.js"), "");
            File.WriteAllText(Path.Combine(tempDirectory, "file2.js"), "");

            bundles.Add<TestableBundle>("~/path", new[] { "~/file1.js", "~/file2.js" });

            FilesUsedToCreateBundle.ShouldEqual(new[] { "~/file1.js", "~/file2.js" });
        }

        [Fact]
        public void AddWithExplicitFileCreatesBundleThatIsSorted()
        {
            File.WriteAllText(Path.Combine(tempDirectory, "file1.js"), "");

            bundles.Add<TestableBundle>("~/path", new[] { "~/file1.js" });

            bundles["~/path"].IsSorted.ShouldBeTrue();
        }

        [Fact]
        public void WhenAddWithExplicitFileNotStartingWithTilde_ThenAssetFileIsApplicationRelative()
        {
            File.WriteAllText(Path.Combine(tempDirectory, "file1.js"), "");

            bundles.Add<TestableBundle>("~/path", new[] { "file1.js" });

            FilesUsedToCreateBundle[0].ShouldEqual("~/file1.js");
        }

        [Fact]
        public void WhenAddWithExplicitFileNotStartingWithTildeButBundleDirectoryExists_ThenAssetFileIsBundleRelative()
        {
            Directory.CreateDirectory(Path.Combine(tempDirectory, "bundle"));
            File.WriteAllText(PathUtilities.Combine(tempDirectory, "bundle", "file1.js"), "");

            bundles.Add<TestableBundle>("~/bundle", new[] { "file1.js" });

            FilesUsedToCreateBundle[0].ShouldEqual("~/bundle/file1.js");
        }

        [Fact]
        public void WhenWithExplicitFileAndCustomizeAction_ThenCreatedBundleIsCustomized()
        {
            File.WriteAllText(Path.Combine(tempDirectory, "file1.js"), "");

            bundles.Add<TestableBundle>("~/path", new[] { "~/file1.js" }, b => b.PageLocation = "test");

            bundles["~/path"].PageLocation.ShouldEqual("test");
        }
    }
}