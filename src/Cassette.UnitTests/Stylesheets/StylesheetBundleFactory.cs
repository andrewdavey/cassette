using System.Linq;
using Cassette.BundleProcessing;
using Cassette.IO;
using Moq;
using Should;
using Xunit;

namespace Cassette.Stylesheets
{
    public class StylesheetBundleFactory_Tests
    {
        readonly StylesheetBundleFactory factory;
        readonly IBundlePipeline<StylesheetBundle> pipeline;

        public StylesheetBundleFactory_Tests()
        {
            pipeline = Mock.Of<IBundlePipeline<StylesheetBundle>>();
            factory = new StylesheetBundleFactory(pipeline);
        }

        [Fact]
        public void CreateBundleReturnsStylesheetBundleWithDirectorySet()
        {
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
            var bundle = factory.CreateExternalBundle("http://test.com/test.css");
            bundle.ShouldBeType<ExternalStylesheetBundle>();
        }

        [Fact]
        public void CreateBundleAssignsSettingsDefaultProcessor()
        {
            var bundle = factory.CreateBundle("~", Enumerable.Empty<IFile>(), new BundleDescriptor { AssetFilenames = { "*" } });
            bundle.Processor.ShouldBeSameAs(pipeline);
        }
    }
}