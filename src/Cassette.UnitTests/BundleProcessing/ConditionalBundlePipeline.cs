using Cassette.Configuration;
using Moq;
using Xunit;

namespace Cassette.BundleProcessing
{
    public class ConditionalBundlePipeline_Tests
    {
        [Fact]
        public void GivenConditionalPipelineWithConditionThatIsFalse_WhenProcess_ThenDoNotCallChildStep()
        {
            var step = new Mock<IBundleProcessor<TestableBundle>>(); 
            var pipeline = new ConditionalBundlePipeline<TestableBundle>(
                s => false,
                step.Object
            );

            pipeline.Process(new TestableBundle("~"), new CassetteSettings());

            step.Verify(s => s.Process(It.IsAny<TestableBundle>(), It.IsAny<CassetteSettings>()), Times.Never());
        }

        [Fact]
        public void GivenConditionalPipelineWithConditionThatIsTrue_WhenProcess_ThenCallChildStep()
        {
            var step = new Mock<IBundleProcessor<TestableBundle>>();
            var pipeline = new ConditionalBundlePipeline<TestableBundle>(
                s => true,
                step.Object
            );

            var bundle = new TestableBundle("~");
            var settings = new CassetteSettings();
            pipeline.Process(bundle, settings);

            step.Verify(s => s.Process(bundle, settings));
        }
    }
}