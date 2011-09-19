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

namespace Cassette.ModuleProcessing
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
            var module = new Module("~");
            var app = Mock.Of<ICassetteApplication>();
            pipeline.Process(module, app);

            step1.Verify(p => p.Process(module, app));
            step2.Verify(p => p.Process(module, app));
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
                new Pipeline<Module>(null);
            });
        }
    }
}

