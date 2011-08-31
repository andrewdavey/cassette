using System.Collections.Generic;
using Moq;
using Should;
using Xunit;

namespace Cassette.ModuleProcessing
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
            pipeline.Process(new Module("~"), Mock.Of<ICassetteApplication>());
            pipeline.CreatePipelineCalled.ShouldBeTrue();
        }

        [Fact]
        public void ProcessCallsStep()
        {
            var pipeline = new MockPipeline();
            pipeline.Process(new Module("~"), Mock.Of<ICassetteApplication>());
            pipeline.DummyStep.ProcessCalled.ShouldBeTrue();
        }

        [Fact]
        public void WhenRemoveStep_ThenProcessDoesNotCallIt()
        {
            var pipeline = new MockPipeline();
            pipeline.Remove<MockStep>();

            pipeline.Process(new Module("~"), Mock.Of<ICassetteApplication>());

            pipeline.DummyStep.ProcessCalled.ShouldBeFalse();
        }

        [Fact]
        public void WhenReplaceStep_ThenProcessCallsNewStep()
        {
            var pipeline = new MockPipeline();
            var newStep = new MockStep();
            pipeline.Replace<MockStep>(newStep);

            pipeline.Process(new Module("~"), Mock.Of<ICassetteApplication>());

            newStep.ProcessCalled.ShouldBeTrue();
        }

        [Fact]
        public void WhenReplaceStep_ThenProcessDoesNotCallOldStep()
        {
            var pipeline = new MockPipeline();
            var newStep = new MockStep();
            pipeline.Replace<MockStep>(newStep);

            pipeline.Process(new Module("~"), Mock.Of<ICassetteApplication>());

            pipeline.DummyStep.ProcessCalled.ShouldBeFalse();
        }

        [Fact]
        public void WhenInsertAfterStep_TheProcessCallsTheNewStep()
        {
            var pipeline = new MockPipeline();
            var newStep = new MockStep();
            pipeline.InsertAfter<MockStep>(newStep);

            pipeline.Process(new Module("~"), Mock.Of<ICassetteApplication>());

            newStep.ProcessCalled.ShouldBeTrue();
        }

        [Fact]
        public void WhenInsertAfterStep_TheProcessCallsTheNewStepAfterOriginalStep()
        {
            var pipeline = new MockPipeline();
            var newStep = new MockStep();
            pipeline.InsertAfter<MockStep>(newStep);

            pipeline.Process(new Module("~"), Mock.Of<ICassetteApplication>());

            (newStep.CallIndex > pipeline.DummyStep.CallIndex).ShouldBeTrue();
        }

        [Fact]
        public void WhenInsertBeforeStep_TheProcessCallsTheNewStep()
        {
            var pipeline = new MockPipeline();
            var newStep = new MockStep();
            pipeline.InsertBefore<MockStep>(newStep);

            pipeline.Process(new Module("~"), Mock.Of<ICassetteApplication>());

            newStep.ProcessCalled.ShouldBeTrue();
        }

        [Fact]
        public void WhenInsertBeforeStep_TheProcessCallsTheNewStepBeforeOriginalStep()
        {
            var pipeline = new MockPipeline();
            var newStep = new MockStep();
            pipeline.InsertBefore<MockStep>(newStep);

            pipeline.Process(new Module("~"), Mock.Of<ICassetteApplication>());

            (newStep.CallIndex < pipeline.DummyStep.CallIndex).ShouldBeTrue();
        }

        [Fact]
        public void WhenUpdateStep_ThenProcessCallsUpdateAction()
        {
            var pipeline = new MockPipeline();
            pipeline.Update<MockStep>(step => step.Updated = true);

            pipeline.Process(new Module("~"), Mock.Of<ICassetteApplication>());

            pipeline.DummyStep.Updated.ShouldBeTrue();
        }

        [Fact]
        public void WhenPrependStep_ThenProcessCallsTheStepFirst()
        {
            var pipeline = new MockPipeline();
            var step = new MockStep();
            pipeline.Prepend(step);

            pipeline.Process(new Module("~"), Mock.Of<ICassetteApplication>());

            (step.CallIndex < pipeline.DummyStep.CallIndex).ShouldBeTrue();
        }

        [Fact]
        public void WhenAppendStep_ThenProcessCallsTheStepLast()
        {
            var pipeline = new MockPipeline();
            var step = new MockStep();
            pipeline.Append(step);

            pipeline.Process(new Module("~"), Mock.Of<ICassetteApplication>());

            (step.CallIndex > pipeline.DummyStep.CallIndex).ShouldBeTrue();
        }

        [Fact]
        public void CanChainModifications()
        {
            var pipeline = new MockPipeline();
            var step1 = new MockStep();
            var step2 = new MockStep();
            pipeline.Append(step1).Append(step2);

            pipeline.Process(new Module("~"), Mock.Of<ICassetteApplication>());

            step1.CallIndex.ShouldEqual(1);
            step2.CallIndex.ShouldEqual(2);
        }

        class MockPipeline : MutablePipeline<Module>
        {
            protected override IEnumerable<IModuleProcessor<Module>> CreatePipeline(Module module, ICassetteApplication application)
            {
                CreatePipelineCalled = true;
                yield return DummyStep;
            }

            public readonly MockStep DummyStep = new MockStep();
            public bool CreatePipelineCalled;
        }

        class MockStep : IModuleProcessor<Module>
        {
            public void Process(Module module, ICassetteApplication application)
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
}