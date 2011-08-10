using Moq;
using Should;
using Xunit;

namespace Cassette.ModuleProcessing
{
    public class ConditionalStep_Tests
    {
        [Fact]
        public void WhenPredicateReturnsTrue_ThenChildrenAreProcessed()
        {
            var module = new Module("test", Mock.Of<IFileSystem>());
            var app = Mock.Of<ICassetteApplication>();
            var child1 = new Mock<IModuleProcessor<Module>>();
            var child2 = new Mock<IModuleProcessor<Module>>();
            var conditionalStep = new ConditionalStep<Module>((m, a) => true, child1.Object, child2.Object);

            conditionalStep.Process(module, app);

            child1.Verify(c => c.Process(module, app));
            child2.Verify(c => c.Process(module, app));
        }

        [Fact]
        public void WhenPredicateReturnsFalse_ThenChildrenAreNotProcessed()
        {
            var module = new Module("test", Mock.Of<IFileSystem>());
            var app = Mock.Of<ICassetteApplication>();
            var child1 = new Mock<IModuleProcessor<Module>>();
            var child2 = new Mock<IModuleProcessor<Module>>();
            var conditionalStep = new ConditionalStep<Module>((m, a) => false, child1.Object, child2.Object);

            conditionalStep.Process(module, app);

            child1.Verify(c => c.Process(module, app), Times.Never());
            child2.Verify(c => c.Process(module, app), Times.Never());
        }

        [Fact]
        public void ModuleBeingProcessedIsPassedToThePredicate()
        {
            var expected = new Module("test", Mock.Of<IFileSystem>());
            Module actual = null;
            var conditionalStep = new ConditionalStep<Module>(
                (m, a) => { actual = m; return false; }
            );
            
            conditionalStep.Process(expected, Mock.Of<ICassetteApplication>());

            actual.ShouldBeSameAs(expected);
        }

        [Fact]
        public void ApplicationBeingProcessedIsPassedToThePredicate()
        {
            var application = Mock.Of<ICassetteApplication>();
            var module = new Module("test", Mock.Of<IFileSystem>());
            ICassetteApplication actual = null;
            var conditionalStep = new ConditionalStep<Module>(
                (m, a) => { actual = a; return false; }
            );

            conditionalStep.Process(module, application);

            actual.ShouldBeSameAs(application);
        }
    }
}