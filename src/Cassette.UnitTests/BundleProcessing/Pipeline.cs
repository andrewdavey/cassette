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

using System;
using Moq;
using Should;
using Xunit;

namespace Cassette.BundleProcessing
{
    public class Pipeline_WithTwoProcessors
    {
        public Pipeline_WithTwoProcessors()
        {
            step1 = new Mock<IBundleProcessor<Bundle>>();
            step2 = new Mock<IBundleProcessor<Bundle>>();
            pipeline = new Pipeline<Bundle>(step1.Object, step2.Object);
        }

        readonly Pipeline<Bundle> pipeline;
        readonly Mock<IBundleProcessor<Bundle>> step1;
        readonly Mock<IBundleProcessor<Bundle>> step2;

        [Fact]
        public void Pipeline_IsABundleProcessor()
        {
            pipeline.ShouldImplement<IBundleProcessor<Bundle>>();
        }

        [Fact]
        public void WhenProcess_ThenEachStepIsCalled()
        {
            var bundle = new TestableBundle("~");
            var app = Mock.Of<ICassetteApplication>();
            pipeline.Process(bundle, app);

            step1.Verify(p => p.Process(bundle, app));
            step2.Verify(p => p.Process(bundle, app));
        }
    }

    public class Pipeline_ConstructorConstraints
    {
        [Fact]
        public void EachStepCannotBeNull()
        {
            Assert.Throws<ArgumentException>(delegate
            {
                new Pipeline<Bundle>((IBundleProcessor<Bundle>)null);
            });
        }

        [Fact]
        public void StepArrayCannotBeNull()
        {
            Assert.Throws<ArgumentNullException>(delegate
            {
                new Pipeline<Bundle>(null);
            });
        }
    }
}

