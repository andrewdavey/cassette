using System.Collections.Generic;
using System.Linq;
using Cassette.Configuration;
using Moq;
using Should;
using Xunit;

namespace Cassette.BundleProcessing
{
    public class MutablePipeline_Tests
    {
        public MutablePipeline_Tests()
        {
            MockStep.count = 0;
        }

        [Fact]
        public void ProcessCallsCreatePipeline()
        {
            var pipeline = new MockPipeline();
            pipeline.Process(new TestableBundle("~"), new CassetteSettings(""));
            pipeline.CreatePipelineCalled.ShouldBeTrue();
        }

        [Fact]
        public void ProcessCallsStep()
        {
            var pipeline = new MockPipeline();
            pipeline.Process(new TestableBundle("~"), new CassetteSettings(""));
            pipeline.DummyStep.ProcessCalled.ShouldBeTrue();
        }

        [Fact]
        public void WhenRemoveStep_ThenProcessDoesNotCallIt()
        {
            var pipeline = new MockPipeline();
            pipeline.Remove<MockStep>();

            pipeline.Process(new TestableBundle("~"), new CassetteSettings(""));

            pipeline.DummyStep.ProcessCalled.ShouldBeFalse();
        }

        [Fact]
        public void WhenReplaceStep_ThenProcessCallsNewStep()
        {
            var pipeline = new MockPipeline();
            var newStep = new MockStep();
            pipeline.Replace<MockStep>(newStep);

            pipeline.Process(new TestableBundle("~"), new CassetteSettings(""));

            newStep.ProcessCalled.ShouldBeTrue();
        }

        [Fact]
        public void WhenReplaceStep_ThenProcessDoesNotCallOldStep()
        {
            var pipeline = new MockPipeline();
            var newStep = new MockStep();
            pipeline.Replace<MockStep>(newStep);

            pipeline.Process(new TestableBundle("~"), new CassetteSettings(""));

            pipeline.DummyStep.ProcessCalled.ShouldBeFalse();
        }

        [Fact]
        public void WhenInsertAfterStep_TheProcessCallsTheNewStep()
        {
            var pipeline = new MockPipeline();
            var newStep = new MockStep();
            pipeline.InsertAfter<MockStep>(newStep);

            pipeline.Process(new TestableBundle("~"), new CassetteSettings(""));

            newStep.ProcessCalled.ShouldBeTrue();
        }

        [Fact]
        public void WhenInsertAfterStep_TheProcessCallsTheNewStepAfterOriginalStep()
        {
            var pipeline = new MockPipeline();
            var newStep = new MockStep();
            pipeline.InsertAfter<MockStep>(newStep);

            pipeline.Process(new TestableBundle("~"), new CassetteSettings(""));

            (newStep.CallIndex > pipeline.DummyStep.CallIndex).ShouldBeTrue();
        }

        [Fact]
        public void WhenInsertBeforeStep_TheProcessCallsTheNewStep()
        {
            var pipeline = new MockPipeline();
            var newStep = new MockStep();
            pipeline.InsertBefore<MockStep>(newStep);

            pipeline.Process(new TestableBundle("~"), new CassetteSettings(""));

            newStep.ProcessCalled.ShouldBeTrue();
        }

        [Fact]
        public void WhenInsertBeforeStep_TheProcessCallsTheNewStepBeforeOriginalStep()
        {
            var pipeline = new MockPipeline();
            var newStep = new MockStep();
            pipeline.InsertBefore<MockStep>(newStep);

            pipeline.Process(new TestableBundle("~"), new CassetteSettings(""));

            (newStep.CallIndex < pipeline.DummyStep.CallIndex).ShouldBeTrue();
        }

        [Fact]
        public void WhenUpdateStep_ThenProcessCallsUpdateAction()
        {
            var pipeline = new MockPipeline();
            pipeline.Update<MockStep>(step => step.Updated = true);

            pipeline.Process(new TestableBundle("~"), new CassetteSettings(""));

            pipeline.DummyStep.Updated.ShouldBeTrue();
        }

        [Fact]
        public void WhenPrependStep_ThenProcessCallsTheStepFirst()
        {
            var pipeline = new MockPipeline();
            var step = new MockStep();
            pipeline.Prepend(step);

            pipeline.Process(new TestableBundle("~"), new CassetteSettings(""));

            (step.CallIndex < pipeline.DummyStep.CallIndex).ShouldBeTrue();
        }

        [Fact]
        public void WhenAppendStep_ThenProcessCallsTheStepLast()
        {
            var pipeline = new MockPipeline();
            var step = new MockStep();
            pipeline.Append(step);

            pipeline.Process(new TestableBundle("~"), new CassetteSettings(""));

            (step.CallIndex > pipeline.DummyStep.CallIndex).ShouldBeTrue();
        }

        [Fact]
        public void CanChainModifications()
        {
            var pipeline = new MockPipeline();
            var step1 = new MockStep();
            var step2 = new MockStep();
            pipeline.Append(step1).Append(step2);

            pipeline.Process(new TestableBundle("~"), new CassetteSettings(""));

            step1.CallIndex.ShouldEqual(1);
            step2.CallIndex.ShouldEqual(2);
        }

        class MockPipeline : MutablePipeline<Bundle>
        {
            protected override IEnumerable<IBundleProcessor<Bundle>> CreatePipeline(Bundle bundle, CassetteSettings settings)
            {
                CreatePipelineCalled = true;
                yield return DummyStep;
            }

            public readonly MockStep DummyStep = new MockStep();
            public bool CreatePipelineCalled;
        }

        class MockStep : IBundleProcessor<Bundle>
        {
            public void Process(Bundle bundle, CassetteSettings settings)
            {
                CallIndex = count++;
                ProcessCalled = true;
            }

            public bool ProcessCalled;
            public int CallIndex;
            public bool Updated;

            public static int count;
        }
    }

    public class MutablePipeline_ChainingApi_Tests
    {
        IBundleProcessor<Bundle> step = Mock.Of<IBundleProcessor<Bundle>>();
        
        [Fact]
        public void AppendReturnsPipeline()
        {
            var pipeline = new TestablePipeline();
            pipeline.Append(step).ShouldBeSameAs(pipeline);
        }

        [Fact]
        public void PrependReturnsPipeline()
        {
            var pipeline = new TestablePipeline();
            pipeline.Prepend(step).ShouldBeSameAs(pipeline);
        }

        [Fact]
        public void RemoveReturnsPipeline()
        {
            var pipeline = new TestablePipeline();
            pipeline.Remove<IBundleProcessor<Bundle>>().ShouldBeSameAs(pipeline);
        }

        [Fact]
        public void ReplaceReturnsPipeline()
        {
            var pipeline = new TestablePipeline();
            pipeline.Replace<IBundleProcessor<Bundle>>(step).ShouldBeSameAs(pipeline);
        }

        [Fact]
        public void InsertAfterReturnsPipeline()
        {
            var pipeline = new TestablePipeline();
            pipeline.InsertAfter<IBundleProcessor<Bundle>>(step).ShouldBeSameAs(pipeline);
        }

        [Fact]
        public void InsertBeforeReturnsPipeline()
        {
            var pipeline = new TestablePipeline();
            pipeline.InsertBefore<IBundleProcessor<Bundle>>(step).ShouldBeSameAs(pipeline);
        }

        [Fact]
        public void UpdateReturnsPipeline()
        {
            var pipeline = new TestablePipeline();
            pipeline.Update<IBundleProcessor<Bundle>>(_ => { }).ShouldBeSameAs(pipeline);
        }

        class TestablePipeline : MutablePipeline<Bundle>
        {
            protected override IEnumerable<IBundleProcessor<Bundle>> CreatePipeline(Bundle bundle, CassetteSettings settings)
            {
                return Enumerable.Empty<IBundleProcessor<Bundle>>();
            }
        }
    }
}
