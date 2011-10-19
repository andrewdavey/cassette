using Should;
using Xunit;
using System.IO;

namespace Cassette.Scripts
{
    public class ScriptBundle_DefaultAssetSource_Tests
    {
        readonly BundleDirectoryInitializer initializer;

        public ScriptBundle_DefaultAssetSource_Tests()
        {
            var bundle = new ScriptBundle("~/test");
            initializer = bundle.BundleInitializers[0] as BundleDirectoryInitializer;
        }

        [Fact]
        public void PathEqualsBundlePath()
        {
            initializer.Path.ShouldEqual("~/test");
        }

        [Fact]
        public void FilePatternIsJsAndCoffee()
        {
            initializer.FilePattern.ShouldEqual("*.js;*.coffee");
        }

        [Fact]
        public void ExcludeMatchesVSDocFiles()
        {
            initializer.ExcludeFilePath.IsMatch("foo-vsdoc.js").ShouldBeTrue();
            initializer.ExcludeFilePath.IsMatch("foo.js").ShouldBeFalse();
        }

        [Fact]
        public void SearchOptionIsAllDirectories()
        {
            initializer.SearchOption.ShouldEqual(SearchOption.AllDirectories);
        }
    }
}