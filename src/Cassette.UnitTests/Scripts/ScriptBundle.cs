using System.IO;
using Moq;
using Should;
using Xunit;

namespace Cassette.Scripts
{
    public class ScriptBundle_Tests
    {
        [Fact]
        public void RenderCallsRenderer()
        {
            var bundle = new ScriptBundle("~");
            var renderer = new Mock<IBundleHtmlRenderer<ScriptBundle>>(); 
            bundle.Renderer = renderer.Object;

            bundle.Render();

            renderer.Verify(r => r.Render(bundle));
        }
    }

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