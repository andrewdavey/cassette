using Moq;
using Should;
using TinyIoC;
using Xunit;

namespace Cassette.BundleProcessing
{
    public class BundlePipeline_Tests
    {
        readonly BundlePipeline<TestableBundle> pipeline;
        readonly Mock<IBundleProcessor<TestableBundle>> step;
        readonly TinyIoCContainer container;

        public BundlePipeline_Tests()
        {
            container = new TinyIoCContainer();
            pipeline = new TestableBundlePipeline(container);
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

        public void InsertTypedStepCreatesStepFromContainer()
        {
            container.Register<TestStep>();
            pipeline.Insert<TestStep>(0);
            pipeline[0].ShouldBeType<TestStep>();
        }

        [Fact]
        public void InsertUsingDelegateFactoryCreatesFactoryFromContainer()
        {
            container.Register<TestStepWithData>();
            pipeline.Insert<TestStepWithData.Factory>(0, factory => factory(42));
            ((TestStepWithData)pipeline[0]).Data.ShouldEqual(42);
        }

        class TestStep : IBundleProcessor<TestableBundle>
        {
            public void Process(TestableBundle bundle)
            {
                throw new System.NotImplementedException();
            }
        }

        class TestStepWithData : IBundleProcessor<TestableBundle>
        {
            public readonly int Data;

            public delegate TestStepWithData Factory(int data);

            public TestStepWithData(int data)
            {
                Data = data;
            }

            public void Process(TestableBundle bundle)
            {
                throw new System.NotImplementedException();
            }
        }
    }
}
