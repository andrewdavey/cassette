using Moq;
using Should;
using Xunit;
using System;

namespace Cassette
{
    public class Pipeline_WithTwoProcessors
    {
        public Pipeline_WithTwoProcessors()
        {
            step1 = new Mock<IModuleProcessor<Module>>();
            step2 = new Mock<IModuleProcessor<Module>>();
            pipeline = new Pipeline<Module>(step1.Object, step2.Object);
        }

        readonly Pipeline<Module> pipeline;
        readonly Mock<IModuleProcessor<Module>> step1;
        readonly Mock<IModuleProcessor<Module>> step2;

        [Fact]
        public void Pipeline_IsAModuleProcessor()
        {
            pipeline.ShouldImplement<IModuleProcessor<Module>>();
        }

        [Fact]
        public void WhenProcess_ThenEachStepIsCalled()
        {
            var module = new Module("c:\\");
            pipeline.Process(module);

            step1.Verify(p => p.Process(module));
            step2.Verify(p => p.Process(module));
        }
    }

    public class Pipeline_ConstructorConstraints
    {
        [Fact]
        public void EachStepCannotBeNull()
        {
            Assert.Throws<ArgumentException>(delegate
            {
                new Pipeline<Module>((IModuleProcessor<Module>)null);
            });
        }

        [Fact]
        public void StepArrayCannotBeNull()
        {
            Assert.Throws<ArgumentNullException>(delegate
            {
                new Pipeline<Module>((IModuleProcessor<Module>[])null);
            });
        }
    }
}
