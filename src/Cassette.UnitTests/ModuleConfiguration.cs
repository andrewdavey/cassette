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
using System.Collections.Generic;
using Cassette.IO;
using Should;
using Xunit;
using Moq;

namespace Cassette
{
    public class ModuleConfiguration_Tests
    {
        [Fact]
        public void GivenModuleHasUrlReference_ThenCreateModuleContainersGeneratesExternalModuleForTheUrl()
        {
            var module = new Module("~/test");
            module.AddReferences(new[] { "http://test.com/api.js" });

            var externalModule = new Module("http://test.com/api.js");
            var moduleFactory = new Mock<IModuleFactory<Module>>();
            moduleFactory.Setup(f => f.CreateExternalModule("http://test.com/api.js"))
                .Returns(externalModule);
            var moduleFactories = new Dictionary<Type, object>
            {
                { typeof(Module), moduleFactory.Object }
            };
            var moduleSource = new Mock<IModuleSource<Module>>();
            moduleSource
                .Setup(s => s.GetModules(It.IsAny<IModuleFactory<Module>>(), It.IsAny<ICassetteApplication>()))
                .Returns(new[] { module });

            var config = new ModuleConfiguration(
                Mock.Of<ICassetteApplication>(),
                Mock.Of<IDirectory>(),
                Mock.Of<IDirectory>(),
                moduleFactories, 
                ""
            );
            config.Add(moduleSource.Object);

            var containers = config.CreateModuleContainers(false, "");
            var generatedModule = containers[typeof(Module)].FindModuleContainingPath("http://test.com/api.js");
            generatedModule.ShouldBeSameAs(externalModule);
        }
    }
}

