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

namespace Cassette.BundleProcessing
{
    public class ConditionalStep_Tests
    {
        [Fact]
        public void WhenPredicateReturnsTrue_ThenChildrenAreProcessed()
        {
            var bundle = new Bundle("~/test");
            var app = Mock.Of<ICassetteApplication>();
            var child1 = new Mock<IBundleProcessor<Bundle>>();
            var child2 = new Mock<IBundleProcessor<Bundle>>();
            var conditionalStep = new ConditionalStep<Bundle>((m, a) => true, child1.Object, child2.Object);

            conditionalStep.Process(bundle, app);

            child1.Verify(c => c.Process(bundle, app));
            child2.Verify(c => c.Process(bundle, app));
        }

        [Fact]
        public void WhenPredicateReturnsFalse_ThenChildrenAreNotProcessed()
        {
            var bundle = new Bundle("~/test");
            var app = Mock.Of<ICassetteApplication>();
            var child1 = new Mock<IBundleProcessor<Bundle>>();
            var child2 = new Mock<IBundleProcessor<Bundle>>();
            var conditionalStep = new ConditionalStep<Bundle>((m, a) => false, child1.Object, child2.Object);

            conditionalStep.Process(bundle, app);

            child1.Verify(c => c.Process(bundle, app), Times.Never());
            child2.Verify(c => c.Process(bundle, app), Times.Never());
        }

        [Fact]
        public void BundleBeingProcessedIsPassedToThePredicate()
        {
            var expected = new Bundle("~/test");
            Bundle actual = null;
            var conditionalStep = new ConditionalStep<Bundle>(
                (m, a) => { actual = m; return false; }
            );
            
            conditionalStep.Process(expected, Mock.Of<ICassetteApplication>());

            actual.ShouldBeSameAs(expected);
        }

        [Fact]
        public void ApplicationBeingProcessedIsPassedToThePredicate()
        {
            var application = Mock.Of<ICassetteApplication>();
            var bundle = new Bundle("~/test");
            ICassetteApplication actual = null;
            var conditionalStep = new ConditionalStep<Bundle>(
                (m, a) => { actual = a; return false; }
            );

            conditionalStep.Process(bundle, application);

            actual.ShouldBeSameAs(application);
        }
    }
}
