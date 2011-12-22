using System.Linq;
using Cassette.IO;
using Should;
using Xunit;

namespace Cassette.Stylesheets
{
    public class StylesheetBundleFactory_Tests
    {
        [Fact]
        public void CreateBundleReturnsStylesheetBundleWithDirectorySet()
        {
            var factory = new StylesheetBundleFactory();
            var bundle = factory.CreateBundle(
                "~/test",
                Enumerable.Empty<IFile>(),
                new BundleDescriptor { AssetFilenames = { "*" } }
            );
            bundle.Path.ShouldEqual("~/test");
        }

        [Fact]
        public void CreateBundleWithUrlCreatesExternalBundle()
        {
            var factory = new StylesheetBundleFactory();
            var bundle = factory.CreateExternalBundle("http://test.com/test.css");
            bundle.ShouldBeType<ExternalStylesheetBundle>();
        }
    }
}