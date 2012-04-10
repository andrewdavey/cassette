using Cassette.Configuration;
using Moq;
using Xunit;

namespace Cassette.BundleProcessing
{
    public class BundlePipeline_Tests
    {
        readonly BundlePipeline<TestableBundle> pipeline;
        readonly Mock<IBundleProcessor<TestableBundle>> step;

        public BundlePipeline_Tests()
        {
            pipeline = new BundlePipeline<TestableBundle>();
            step = new Mock<IBundleProcessor<TestableBundle>>();
            pipeline.Add(step.Object);
        }

        [Fact]
        public void ProcessCallsStep()
        {
            var bundle = new TestableBundle("~");
            var settings = new CassetteSettings();

            pipeline.Process(bundle, settings);

            step.Verify(s => s.Process(bundle, settings));
        }
    }
}
