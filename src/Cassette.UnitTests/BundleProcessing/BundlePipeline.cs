using Moq;
using TinyIoC;
using Xunit;

namespace Cassette.BundleProcessing
{
    public class BundlePipeline_Tests
    {
        readonly BundlePipeline<TestableBundle> pipeline;
        readonly Mock<IBundleProcessor<TestableBundle>> step;

        public BundlePipeline_Tests()
        {
            pipeline = new TestableBundlePipeline(new TinyIoCContainer());
            step = new Mock<IBundleProcessor<TestableBundle>>();
            pipeline.Add(step.Object);
        }

        [Fact]
        public void ProcessCallsStep()
        {
            var bundle = new TestableBundle("~");

            pipeline.Process(bundle);

            step.Verify(s => s.Process(bundle));
        }
    }
}
