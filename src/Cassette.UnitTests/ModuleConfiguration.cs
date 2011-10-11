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
    public class BundleConfiguration_Tests
    {
        [Fact]
        public void GivenBundleHasUrlReference_ThenCreateBundleContainersGeneratesExternalBundleForTheUrl()
        {
            var bundle = new Bundle("~/test");
            bundle.AddReferences(new[] { "http://test.com/api.js" });

            var externalBundle = new Bundle("http://test.com/api.js");
            var bundleFactory = new Mock<IBundleFactory<Bundle>>();
            bundleFactory.Setup(f => f.CreateExternalBundle("http://test.com/api.js"))
                .Returns(externalBundle);
            var bundleFactories = new Dictionary<Type, object>
            {
                { typeof(Bundle), bundleFactory.Object }
            };
            var bundleSource = new Mock<IBundleSource<Bundle>>();
            bundleSource
                .Setup(s => s.GetBundles(It.IsAny<IBundleFactory<Bundle>>(), It.IsAny<ICassetteApplication>()))
                .Returns(new[] { bundle });

            var config = new BundleConfiguration(
                Mock.Of<ICassetteApplication>(),
                Mock.Of<IDirectory>(),
                Mock.Of<IDirectory>(),
                bundleFactories, 
                ""
            );
            config.Add(bundleSource.Object);

            var containers = config.CreateBundleContainers(false, "");
            var generatedBundle = containers[typeof(Bundle)].FindBundleContainingPath("http://test.com/api.js");
            generatedBundle.ShouldBeSameAs(externalBundle);
        }
    }
}

