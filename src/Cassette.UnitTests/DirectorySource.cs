using System.Linq;
using Should;
using Xunit;

namespace Cassette
{
    // DirectorySource shares most of it's implementation with PerSubDirectorySource.
    // So please see the PerSubDirectorySource tests for more thorough testing of this
    // common code.

    public class DirectorySource_Tests : ModuleSourceTestBase
    {
        [Fact]
        public void GivenDirectoryWithFile_ThenGetModulesReturnsModuleWithAsset()
        {
            GivenFiles("module-a/1.js");

            var source = new DirectorySource<Module>("module-a");

            var result = source.GetModules(moduleFactory, application);
            result.Count().ShouldEqual(1);
            result.First().Assets.Count.ShouldEqual(1);
        }

        [Fact]
        public void GivenTwoDirectories_ThenGetModulesReturnsTwoModules()
        {
            GivenFiles("module-a/1.js", "module-b/2.js");

            var source = new DirectorySource<Module>("module-a", "module-b");

            var result = source.GetModules(moduleFactory, application);
            result.Count().ShouldEqual(2);
        }
    }
}