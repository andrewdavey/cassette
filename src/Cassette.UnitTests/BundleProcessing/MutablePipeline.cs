#region License
/*
Copyright 2011 Andrew Davey

This file is part of Cassette.

Cassette is free software: you can redistribute it and/or modify it under the 
terms of the GNU General Public License as published by the Free Software 
Foundation, either version 3 of the License, or (at your option) any later 
version.

Cassette is distributed in the hope that it will be useful, but WITHOUT ANY 
WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS 
FOR A PARTICULAR PURPOSE. See the GNU General Public License for more details.

You should have received a copy of the GNU General Public License along with 
Cassette. If not, see http://www.gnu.org/licenses/.
*/
#endregion

using System.Collections.Generic;
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
            pipeline.Process(new TestableBundle("~"), Mock.Of<ICassetteApplication>());
            pipeline.CreatePipelineCalled.ShouldBeTrue();
        }

        [Fact]
        public void ProcessCallsStep()
        {
            var pipeline = new MockPipeline();
            pipeline.Process(new TestableBundle("~"), Mock.Of<ICassetteApplication>());
            pipeline.DummyStep.ProcessCalled.ShouldBeTrue();
        }

        [Fact]
        public void WhenRemoveStep_ThenProcessDoesNotCallIt()
        {
            var pipeline = new MockPipeline();
            pipeline.Remove<MockStep>();

            pipeline.Process(new TestableBundle("~"), Mock.Of<ICassetteApplication>());

            pipeline.DummyStep.ProcessCalled.ShouldBeFalse();
        }

        [Fact]
        public void WhenReplaceStep_ThenProcessCallsNewStep()
        {
            var pipeline = new MockPipeline();
            var newStep = new MockStep();
            pipeline.Replace<MockStep>(newStep);

            pipeline.Process(new TestableBundle("~"), Mock.Of<ICassetteApplication>());

            newStep.ProcessCalled.ShouldBeTrue();
        }

        [Fact]
        public void WhenReplaceStep_ThenProcessDoesNotCallOldStep()
        {
            var pipeline = new MockPipeline();
            var newStep = new MockStep();
            pipeline.Replace<MockStep>(newStep);

            pipeline.Process(new TestableBundle("~"), Mock.Of<ICassetteApplication>());

            pipeline.DummyStep.ProcessCalled.ShouldBeFalse();
        }

        [Fact]
        public void WhenInsertAfterStep_TheProcessCallsTheNewStep()
        {
            var pipeline = new MockPipeline();
            var newStep = new MockStep();
            pipeline.InsertAfter<MockStep>(newStep);

            pipeline.Process(new TestableBundle("~"), Mock.Of<ICassetteApplication>());

            newStep.ProcessCalled.ShouldBeTrue();
        }

        [Fact]
        public void WhenInsertAfterStep_TheProcessCallsTheNewStepAfterOriginalStep()
        {
            var pipeline = new MockPipeline();
            var newStep = new MockStep();
            pipeline.InsertAfter<MockStep>(newStep);

            pipeline.Process(new TestableBundle("~"), Mock.Of<ICassetteApplication>());

            (newStep.CallIndex > pipeline.DummyStep.CallIndex).ShouldBeTrue();
        }

        [Fact]
        public void WhenInsertBeforeStep_TheProcessCallsTheNewStep()
        {
            var pipeline = new MockPipeline();
            var newStep = new MockStep();
            pipeline.InsertBefore<MockStep>(newStep);

            pipeline.Process(new TestableBundle("~"), Mock.Of<ICassetteApplication>());

            newStep.ProcessCalled.ShouldBeTrue();
        }

        [Fact]
        public void WhenInsertBeforeStep_TheProcessCallsTheNewStepBeforeOriginalStep()
        {
            var pipeline = new MockPipeline();
            var newStep = new MockStep();
            pipeline.InsertBefore<MockStep>(newStep);

            pipeline.Process(new TestableBundle("~"), Mock.Of<ICassetteApplication>());

            (newStep.CallIndex < pipeline.DummyStep.CallIndex).ShouldBeTrue();
        }

        [Fact]
        public void WhenUpdateStep_ThenProcessCallsUpdateAction()
        {
            var pipeline = new MockPipeline();
            pipeline.Update<MockStep>(step => step.Updated = true);

            pipeline.Process(new TestableBundle("~"), Mock.Of<ICassetteApplication>());

            pipeline.DummyStep.Updated.ShouldBeTrue();
        }

        [Fact]
        public void WhenPrependStep_ThenProcessCallsTheStepFirst()
        {
            var pipeline = new MockPipeline();
            var step = new MockStep();
            pipeline.Prepend(step);

            pipeline.Process(new TestableBundle("~"), Mock.Of<ICassetteApplication>());

            (step.CallIndex < pipeline.DummyStep.CallIndex).ShouldBeTrue();
        }

        [Fact]
        public void WhenAppendStep_ThenProcessCallsTheStepLast()
        {
            var pipeline = new MockPipeline();
            var step = new MockStep();
            pipeline.Append(step);

            pipeline.Process(new TestableBundle("~"), Mock.Of<ICassetteApplication>());

            (step.CallIndex > pipeline.DummyStep.CallIndex).ShouldBeTrue();
        }

        [Fact]
        public void CanChainModifications()
        {
            var pipeline = new MockPipeline();
            var step1 = new MockStep();
            var step2 = new MockStep();
            pipeline.Append(step1).Append(step2);

            pipeline.Process(new TestableBundle("~"), Mock.Of<ICassetteApplication>());

            step1.CallIndex.ShouldEqual(1);
            step2.CallIndex.ShouldEqual(2);
        }

        class MockPipeline : MutablePipeline<Bundle>
        {
            protected override IEnumerable<IBundleProcessor<Bundle>> CreatePipeline(Bundle bundle, ICassetteApplication application)
            {
                CreatePipelineCalled = true;
                yield return DummyStep;
            }

            public readonly MockStep DummyStep = new MockStep();
            public bool CreatePipelineCalled;
        }

        class MockStep : IBundleProcessor<Bundle>
        {
            public void Process(Bundle bundle, ICassetteApplication application)
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
