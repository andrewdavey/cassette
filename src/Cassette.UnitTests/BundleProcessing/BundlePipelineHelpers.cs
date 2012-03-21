using System.Linq;
using Moq;
using Should;
using Xunit;

namespace Cassette.BundleProcessing
{
    public class BundlePipelineHelpers_Tests
    {
        public class WhenInsertAfterSingleExistingStep
        {
            readonly BundlePipeline<TestableBundle> pipeline;
            readonly AssignHash firstStep;
            readonly Mock<IBundleProcessor<TestableBundle>> newStep;

            public WhenInsertAfterSingleExistingStep()
            {
                pipeline = new BundlePipeline<TestableBundle>();
                firstStep = new AssignHash();
                pipeline.Add(firstStep);

                newStep = new Mock<IBundleProcessor<TestableBundle>>();
                pipeline.InsertAfter<AssignHash, TestableBundle>(newStep.Object);
            }

            [Fact]
            public void PipelineCountEquals2()
            {
                pipeline.Count.ShouldEqual(2);
            }

            [Fact]
            public void NewStepIsAfterFirstStep()
            {
                pipeline.ToArray()[0].ShouldBeSameAs(firstStep);
                pipeline.ToArray()[1].ShouldBeSameAs(newStep.Object);
            }
        }

        public class WhenInsertAfterWithMultipleNewSteps
        {
            readonly BundlePipeline<TestableBundle> pipeline;
            readonly IBundleProcessor<TestableBundle> newStep1;
            readonly IBundleProcessor<TestableBundle> newStep2;

            public WhenInsertAfterWithMultipleNewSteps()
            {
                pipeline = new BundlePipeline<TestableBundle>();
                var firstStep = new AssignHash();
                pipeline.Add(firstStep);

                newStep1 = Mock.Of<IBundleProcessor<TestableBundle>>();
                newStep2 = Mock.Of<IBundleProcessor<TestableBundle>>();
                pipeline.InsertAfter<AssignHash, TestableBundle>(newStep1, newStep2);
            }

            [Fact]
            public void NewStepsAreInsertedInOrder()
            {
                pipeline.ElementAt(1).ShouldBeSameAs(newStep1);
                pipeline.ElementAt(2).ShouldBeSameAs(newStep2);
            }
        }

        public class WhenInsertBeforeSingleExistingStep
        {
            readonly BundlePipeline<TestableBundle> pipeline;
            readonly AssignHash firstStep;
            readonly IBundleProcessor<TestableBundle> newStep;

            public WhenInsertBeforeSingleExistingStep()
            {
                pipeline = new BundlePipeline<TestableBundle>();
                firstStep = new AssignHash();
                pipeline.Add(firstStep);

                newStep = Mock.Of<IBundleProcessor<TestableBundle>>();
                pipeline.InsertBefore<AssignHash, TestableBundle>(newStep);
            }

            [Fact]
            public void PipelineCountEquals2()
            {
                pipeline.Count.ShouldEqual(2);
            }

            [Fact]
            public void NewStepIsBeforeFirstStep()
            {
                pipeline.ToArray()[0].ShouldBeSameAs(newStep);
                pipeline.ToArray()[1].ShouldBeSameAs(firstStep);
            }
        }

        public class WhenInsertBeforeWithMultipleNewSteps
        {
            readonly BundlePipeline<TestableBundle> pipeline;
            readonly IBundleProcessor<TestableBundle> newStep1;
            readonly IBundleProcessor<TestableBundle> newStep2;

            public WhenInsertBeforeWithMultipleNewSteps()
            {
                pipeline = new BundlePipeline<TestableBundle>();
                var firstStep = new AssignHash();
                pipeline.Add(firstStep);

                newStep1 = Mock.Of<IBundleProcessor<TestableBundle>>();
                newStep2 = Mock.Of<IBundleProcessor<TestableBundle>>();
                pipeline.InsertBefore<AssignHash, TestableBundle>(newStep1, newStep2);
            }

            [Fact]
            public void NewStepsAreInsertedInOrder()
            {
                pipeline.ElementAt(0).ShouldBeSameAs(newStep1);
                pipeline.ElementAt(1).ShouldBeSameAs(newStep2);
            }
        }
    }
}