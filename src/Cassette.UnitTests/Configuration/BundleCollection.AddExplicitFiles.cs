using System.IO;
using Cassette.IO;
using Cassette.Scripts;
using Should;
using Xunit;

namespace Cassette.Configuration
{
    public class BundleCollection_AddExplicitFiles_Tests
    {
        [Fact]
        public void AddWithExplicitFileCreatesBundleWithAsset()
        {
            using (var temp = new TempDirectory())
            {
                File.WriteAllText(Path.Combine(temp, "file1.js"), "");

                var settings = new CassetteSettings("");
                var bundles = new BundleCollection(settings);
                settings.SourceDirectory = new FileSystemDirectory(temp);

                bundles.Add<ScriptBundle>("~/path", new[] { "~/file1.js" });

                bundles["~/path"].Assets[0].SourceFile.FullPath.ShouldEqual("~/file1.js");
            }
        }

        [Fact]
        public void AddWithExplicitFilesAsParamsArrayCreatesBundleWithAsset()
        {
            using (var temp = new TempDirectory())
            {
                File.WriteAllText(Path.Combine(temp, "file1.js"), "");
                File.WriteAllText(Path.Combine(temp, "file2.js"), "");

                var settings = new CassetteSettings("");
                var bundles = new BundleCollection(settings);
                settings.SourceDirectory = new FileSystemDirectory(temp);

                bundles.Add<ScriptBundle>("~/path", "~/file1.js", "~/file2.js");

                bundles["~/path"].Assets[0].SourceFile.FullPath.ShouldEqual("~/file1.js");
                bundles["~/path"].Assets[1].SourceFile.FullPath.ShouldEqual("~/file2.js");
            }
        }

        [Fact]
        public void AddWithTwoExplicitFileCreatesBundleWithTwoAssets()
        {
            using (var temp = new TempDirectory())
            {
                File.WriteAllText(Path.Combine(temp, "file1.js"), "");
                File.WriteAllText(Path.Combine(temp, "file2.js"), "");

                var settings = new CassetteSettings("");
                var bundles = new BundleCollection(settings);
                settings.SourceDirectory = new FileSystemDirectory(temp);

                bundles.Add<ScriptBundle>("~/path", new[] { "~/file1.js", "~/file2.js" });

                bundles["~/path"].Assets[0].SourceFile.FullPath.ShouldEqual("~/file1.js");
                bundles["~/path"].Assets[1].SourceFile.FullPath.ShouldEqual("~/file2.js");
            }
        }

        [Fact]
        public void AddWithExplicitFileCreatesBundleThatIsSorted()
        {
            using (var temp = new TempDirectory())
            {
                File.WriteAllText(Path.Combine(temp, "file1.js"), "");

                var settings = new CassetteSettings("");
                var bundles = new BundleCollection(settings);
                settings.SourceDirectory = new FileSystemDirectory(temp);

                bundles.Add<ScriptBundle>("~/path", new[] { "~/file1.js" });

                bundles["~/path"].IsSorted.ShouldBeTrue();
            }
        }

        [Fact]
        public void WhenAddWithExplicitFileNotStartingWithTilde_ThenAssetFileIsApplicationRelative()
        {
            using (var temp = new TempDirectory())
            {
                File.WriteAllText(Path.Combine(temp, "file1.js"), "");

                var settings = new CassetteSettings("");
                var bundles = new BundleCollection(settings);
                settings.SourceDirectory = new FileSystemDirectory(temp);

                bundles.Add<ScriptBundle>("~/path", new[] { "file1.js" });

                bundles["~/path"].Assets[0].SourceFile.FullPath.ShouldEqual("~/file1.js");
            }
        }

        [Fact]
        public void WhenAddWithExplicitFileNotStartingWithTildeButBundleDirectoryExists_ThenAssetFileIsBundleRelative()
        {
            using (var temp = new TempDirectory())
            {
                Directory.CreateDirectory(Path.Combine(temp, "bundle"));
                File.WriteAllText(Path.Combine(temp, "bundle", "file1.js"), "");

                var settings = new CassetteSettings("");
                var bundles = new BundleCollection(settings);
                settings.SourceDirectory = new FileSystemDirectory(temp);

                bundles.Add<ScriptBundle>("~/bundle", new[] { "file1.js" });

                bundles["~/bundle"].Assets[0].SourceFile.FullPath.ShouldEqual("~/bundle/file1.js");
            }
        }

        [Fact]
        public void WhenWithExplicitFileAndCustomizeAction_ThenCreatedBundleIsCustomized()
        {
            using (var temp = new TempDirectory())
            {
                File.WriteAllText(Path.Combine(temp, "file1.js"), "");

                var settings = new CassetteSettings("");
                var bundles = new BundleCollection(settings);
                settings.SourceDirectory = new FileSystemDirectory(temp);

                bundles.Add<ScriptBundle>("~/path", new[] { "~/file1.js" }, b => b.PageLocation = "test");

                bundles["~/path"].PageLocation.ShouldEqual("test");
            }
        }
    }
}