using System.Linq;
using Cassette.BundleProcessing;
using Moq;
using Should;
using Xunit;

namespace Cassette.Configuration
{
    public class BundleDefaults_Tests
    {
        readonly BundleDefaults<TestableBundle> defaults = new BundleDefaults<TestableBundle>();

        [Fact]
        public void HasBundleFactoryProperty()
        {
            var factory = Mock.Of<IBundleFactory<TestableBundle>>();
            defaults.BundleFactory = factory;
            defaults.BundleFactory.ShouldBeSameAs(factory);
        }

        [Fact]
        public void HasBundlePipelineProperty()
        {
            var pipeline = Mock.Of<IBundlePipeline<TestableBundle>>();
            defaults.BundlePipeline = pipeline;
            defaults.BundlePipeline.ShouldBeSameAs(pipeline);
        }

        [Fact]
        public void HasFileSearchProperty()
        {
            var fileSearch = new FileSearch();
            defaults.FileSearch = fileSearch;
            defaults.FileSearch.ShouldBeSameAs(fileSearch);
        }
    }
}