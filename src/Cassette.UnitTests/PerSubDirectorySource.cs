using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using Should;
using Xunit;

namespace Cassette
{
    public class PerSubDirectorySource_Test : ModuleSourceTestBase
    {
        [Fact]
        public void GivenBaseDirectoryHasEmptyDirectory_ThenGetModulesReturnsEmptyModule()
        {
            Directory.CreateDirectory(Path.Combine(root, "scripts", "empty"));

            var source = new PerSubDirectorySource<Module>("scripts");
            var result = source.GetModules(moduleFactory, application);

            var module = result.Modules.First();
            module.Assets.Count.ShouldEqual(0);
        }

        [Fact]
        public void GivenBaseDirectoryWithTwoDirectories_ThenGetModulesReturnsTwoModules()
        {
            GivenFiles("scripts/module-a/1.js", "scripts/module-b/2.js");

            var source = new PerSubDirectorySource<Module>("scripts");
            var result = source.GetModules(moduleFactory, application);
            
            var modules = result.Modules.ToArray();
            modules.Length.ShouldEqual(2);
        }

        [Fact]
        public void GivenMixedFileTypes_WhenFilesFiltered_ThenGetModulesFindsOnlyMatchingFiles()
        {
            GivenFiles("scripts/module-a/1.js", "scripts/module-a/ignored.txt");

            var source = new PerSubDirectorySource<Module>("scripts") { FilePattern = "*.js" };
            var result = source.GetModules(moduleFactory, application);

            var module = result.Modules.First();
            module.Assets.Count.ShouldEqual(1);
        }

        [Fact]
        public void GivenAmbiguousFileFilters_ThenGetModulesFindsFileOnlyOnce()
        {
            GivenFiles("scripts/module-a/1.html");

            var source = new PerSubDirectorySource<Module>("scripts") { FilePattern = "*.htm;*.html" };
            var result = source.GetModules(moduleFactory, application);

            var module = result.Modules.First();
            module.Assets.Count.ShouldEqual(1);
        }

        [Fact]
        public void GivenFilesWeDontWantInModule_WhenExclusionProvided_ThenGetModulesDoesntIncludeExcludedFiles()
        {
            GivenFiles("scripts/module-a/1.js", "scripts/module-a/1-vsdoc.js");

            var source = new PerSubDirectorySource<Module>("scripts") { FilePattern = "*.js" };
            source.Exclude = new Regex("-vsdoc\\.js$");

            var result = source.GetModules(moduleFactory, application);

            var module = result.Modules.First();
            module.Assets.Count.ShouldEqual(1);
        }

        [Fact]
        public void GivenBaseDirectoryDoesNotExist_ThenGetModulesThrowsException()
        {
            var source = new PerSubDirectorySource<Module>("missing");
            Assert.Throws<DirectoryNotFoundException>(delegate
            {
                source.GetModules(moduleFactory, application);
            });
        }
    }
}
