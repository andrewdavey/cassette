using System.Collections.Generic;
using Moq;
using Should;
using Xunit;

namespace Cassette.ModuleProcessing
{
    public class MutablePipeline_Tests
    {
        [Fact]
        public void ProcessCallsCreatePipeline()
        {
            var pipeline = new MockPipeline<Module>();
            pipeline.Process(new Module(""), Mock.Of<ICassetteApplication>());
            pipeline.CreatePipelineCalled.ShouldBeTrue();
        }

        [Fact]
        public void ProcessCallsStep()
        {
            var pipeline = new MockPipeline<Module>();
            pipeline.Process(new Module(""), Mock.Of<ICassetteApplication>());
            pipeline.DummyStep.ProcessCalled.ShouldBeTrue();
        }

        [Fact]
        public void WhenRemoveStep_ThenProcessDoesNotCallIt()
        {
            var pipeline = new MockPipeline<Module>();
            pipeline.Remove<MockStep>();

            pipeline.Process(new Module(""), Mock.Of<ICassetteApplication>());

            pipeline.DummyStep.ProcessCalled.ShouldBeFalse();
        }

        [Fact]
        public void WhenReplaceStep_ThenProcessCallsNewStep()
        {
            var pipeline = new MockPipeline<Module>();
            var newStep = new MockStep();
            pipeline.Replace<MockStep>(newStep);

            pipeline.Process(new Module(""), Mock.Of<ICassetteApplication>());

            newStep.ProcessCalled.ShouldBeTrue();
        }

        [Fact]
        public void WhenReplaceStep_ThenProcessDoesNotCallOldStep()
        {
            var pipeline = new MockPipeline<Module>();
            var newStep = new MockStep();
            pipeline.Replace<MockStep>(newStep);

            pipeline.Process(new Module(""), Mock.Of<ICassetteApplication>());

            pipeline.DummyStep.ProcessCalled.ShouldBeFalse();
        }

        [Fact]
        public void WhenInsertAfterStep_TheProcessCallsTheNewStep()
        {
            var pipeline = new MockPipeline<Module>();
            var newStep = new MockStep();
            pipeline.InsertAfter<MockStep>(newStep);

            pipeline.Process(new Module(""), Mock.Of<ICassetteApplication>());

            newStep.ProcessCalled.ShouldBeTrue();
        }

        [Fact]
        public void WhenInsertAfterStep_TheProcessCallsTheNewStepAfterOriginalStep()
        {
            var pipeline = new MockPipeline<Module>();
            var newStep = new MockStep();
            pipeline.InsertAfter<MockStep>(newStep);

            pipeline.Process(new Module(""), Mock.Of<ICassetteApplication>());

            (newStep.CallIndex > pipeline.DummyStep.CallIndex).ShouldBeTrue();
        }

        [Fact]
        public void WhenInsertBeforeStep_TheProcessCallsTheNewStep()
        {
            var pipeline = new MockPipeline<Module>();
            var newStep = new MockStep();
            pipeline.InsertBefore<MockStep>(newStep);

            pipeline.Process(new Module(""), Mock.Of<ICassetteApplication>());

            newStep.ProcessCalled.ShouldBeTrue();
        }

        [Fact]
        public void WhenInsertBeforeStep_TheProcessCallsTheNewStepBeforeOriginalStep()
        {
            var pipeline = new MockPipeline<Module>();
            var newStep = new MockStep();
            pipeline.InsertBefore<MockStep>(newStep);

            pipeline.Process(new Module(""), Mock.Of<ICassetteApplication>());

            (newStep.CallIndex < pipeline.DummyStep.CallIndex).ShouldBeTrue();
        }

        [Fact]
        public void WhenUpdateStep_ThenProcessCallsUpdateAction()
        {
            var pipeline = new MockPipeline<Module>();
            pipeline.Update<MockStep>(step => step.Updated = true);

            pipeline.Process(new Module(""), Mock.Of<ICassetteApplication>());

            pipeline.DummyStep.Updated.ShouldBeTrue();
        }

        class MockPipeline<T> : MutablePipeline<T>
            where T : Module
        {
            protected override IEnumerable<IModuleProcessor<T>> CreatePipeline(T module, ICassetteApplication application)
            {
                CreatePipelineCalled = true;
                yield return DummyStep;
            }

            public MockStep DummyStep = new MockStep();
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

            static int count;
        }
    }
}