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
            var module = new Module("~/test");
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
            var module = new Module("~/test");
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
            var expected = new Module("~/test");
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
            var module = new Module("~/test");
            ICassetteApplication actual = null;
            var conditionalStep = new ConditionalStep<Module>(
                (m, a) => { actual = a; return false; }
            );

            conditionalStep.Process(module, application);

            actual.ShouldBeSameAs(application);
        }
    }
}
