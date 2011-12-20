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
using System.Linq;
using Cassette.IO;
using Moq;
using Should;
using Xunit;

namespace Cassette.Configuration
{
    public abstract class BundleContainerFactoryTestSuite
    {
        internal abstract IBundleContainerFactory CreateFactory(IDictionary<Type, IBundleFactory<Bundle>> factories);

        [Fact]
        public void WhenCreateWithBundles_ThenItReturnsContainerWithBundles()
        {
            var bundle1 = new TestableBundle("~/test1");
            var bundle2 = new TestableBundle("~/test2");
            var bundles = new[] { bundle1, bundle2 };

            var builder = CreateFactory(new Dictionary<Type, IBundleFactory<Bundle>>());
            var container = builder.Create(bundles, CreateSettings());

            container.FindBundleContainingPath<Bundle>("~/test1").ShouldBeSameAs(bundle1);
            container.FindBundleContainingPath<Bundle>("~/test2").ShouldBeSameAs(bundle2);
        }

        [Fact]
        public void WhenCreateWithBundle_ThenBundleIsProcessed()
        {
            var bundle = new TestableBundle("~/test");
            var settings = CreateSettings();

            var builder = CreateFactory(new Dictionary<Type, IBundleFactory<Bundle>>());
            builder.Create(new[] { bundle }, settings);

            bundle.WasProcessed.ShouldBeTrue();
        }

        [Fact]
        public void WhenCreateWithBundleHavingExternalReference_ThenAnExternalBundleIsAlsoAddedToContainer()
        {
            var externalBundle = new TestableBundle("http://external.com/api.js");
            var bundle = new TestableBundle("~/test");
            bundle.AddReference("http://external.com/api.js");

            var factories = new Dictionary<Type, IBundleFactory<Bundle>>();
            var factory = new Mock<IBundleFactory<Bundle>>();
            factory.Setup(f => f.CreateBundle("http://external.com/api.js", It.IsAny<IEnumerable<IFile>>(), It.IsAny<BundleDescriptor>()))
                   .Returns(externalBundle);
            factories[typeof(TestableBundle)] = factory.Object;

            var builder = CreateFactory(factories);
            var container = builder.Create(new[] { bundle }, CreateSettings());

            container.FindBundleContainingPath<Bundle>("http://external.com/api.js").ShouldBeSameAs(externalBundle);
        }

        [Fact]
        public void WhenExternalModuleReferencedTwice_ThenContainerOnlyHasTheExternalModuleOnce()
        {
            var externalBundle = new TestableBundle("http://external.com/api.js");
            var bundle1 = new TestableBundle("~/test1");
            bundle1.AddReference("http://external.com/api.js");
            var bundle2 = new TestableBundle("~/test2");
            bundle2.AddReference("http://external.com/api.js");
            var bundles = new[] { bundle1, bundle2 };

            var factories = new Dictionary<Type, IBundleFactory<Bundle>>();
            var factory = new Mock<IBundleFactory<Bundle>>();
            factory.Setup(f => f.CreateBundle("http://external.com/api.js", It.IsAny<IEnumerable<IFile>>(), It.IsAny<BundleDescriptor>()))
                   .Returns(externalBundle);
            factories[typeof(TestableBundle)] = factory.Object;

            var builder = CreateFactory(factories);
            var container = builder.Create(bundles, CreateSettings());

            container.Bundles.Count().ShouldEqual(3);
        }

        [Fact]
        public void GivenAssetWithUrlReference_WhenCreate_ThenExternalBundleInContainer()
        {
            var asset = new Mock<IAsset>();
            asset.SetupGet(a => a.References)
                 .Returns(new[] { new AssetReference("http://test.com/", asset.Object, -1, AssetReferenceType.Url) });

            var externalBundle = new TestableBundle("http://test.com/");
            var factory = new Mock<IBundleFactory<Bundle>>();
            factory.Setup(f => f.CreateBundle("http://test.com/", It.IsAny<IEnumerable<IFile>>(), It.IsAny<BundleDescriptor>()))
                   .Returns(externalBundle);
            var factories = new Dictionary<Type, IBundleFactory<Bundle>>();
            factories[typeof(TestableBundle)] = factory.Object;
            var containerFactory = CreateFactory(factories);

            var bundle = new TestableBundle("~/test");
            bundle.Assets.Add(asset.Object);
            var settings = CreateSettings();
            var container = containerFactory.Create(new[] { bundle }, settings);

            container.FindBundleContainingPath<Bundle>("http://test.com/").ShouldBeSameAs(externalBundle);
        }

        [Fact]
        public void GivenAssetWithUrlReferenceAndSameBundleLevelUrlReference_WhenCreate_ThenExternalBundleInContainer()
        {
            var asset = new Mock<IAsset>();
            asset.SetupGet(a => a.References)
                 .Returns(new[] { new AssetReference("http://test.com/", asset.Object, -1, AssetReferenceType.Url) });

            var externalBundle = new TestableBundle("http://test.com/");
            var factory = new Mock<IBundleFactory<Bundle>>();
            factory.Setup(f => f.CreateBundle("http://test.com/", It.IsAny<IEnumerable<IFile>>(), It.IsAny<BundleDescriptor>()))
                   .Returns(externalBundle);
            var factories = new Dictionary<Type, IBundleFactory<Bundle>>();
            factories[typeof(TestableBundle)] = factory.Object;
            var containerFactory = CreateFactory(factories);

            var bundle = new TestableBundle("~/test");
            bundle.AddReference("http://test.com/");
            bundle.Assets.Add(asset.Object);
            var settings = CreateSettings();
            var container = containerFactory.Create(new[] { bundle }, settings);

            container.FindBundleContainingPath<Bundle>("http://test.com/").ShouldBeSameAs(externalBundle);
        }

        [Fact]
        public void GivenAssetWithAdHocUrlReference_WhenCreate_ThenExternalBundleIsCreatedAndProcessed()
        {
            var asset = new Mock<IAsset>();
            asset.SetupGet(a => a.References)
                 .Returns(new[] { new AssetReference("http://test.com/", asset.Object, -1, AssetReferenceType.Url) });

            var externalBundle = new TestableBundle("http://test.com/");
            var factory = new Mock<IBundleFactory<Bundle>>();
            factory.Setup(f => f.CreateBundle("http://test.com/", It.IsAny<IEnumerable<IFile>>(), It.IsAny<BundleDescriptor>()))
                   .Returns(externalBundle);
            var factories = new Dictionary<Type, IBundleFactory<Bundle>>();
            factories[typeof(TestableBundle)] = factory.Object;
            var containerFactory = CreateFactory(factories);

            var bundle = new TestableBundle("~/test");
            bundle.Assets.Add(asset.Object);
            var settings = CreateSettings();
            
            containerFactory.Create(new[] { bundle }, settings);

            externalBundle.WasProcessed.ShouldBeTrue();
        }


        [Fact]
        public void GivenBundleWithAdHocUrlReference_WhenCreate_ThenExternalBundleIsCreatedAndProcessed()
        {
            var externalBundle = new TestableBundle("http://test.com/");
            var factory = new Mock<IBundleFactory<Bundle>>();
            factory.Setup(f => f.CreateBundle("http://test.com/", It.IsAny<IEnumerable<IFile>>(), It.IsAny<BundleDescriptor>()))
                   .Returns(externalBundle);
            var factories = new Dictionary<Type, IBundleFactory<Bundle>>();
            factories[typeof(TestableBundle)] = factory.Object;
            var containerFactory = CreateFactory(factories);

            var bundle = new TestableBundle("~/test");
            bundle.AddReference("http://test.com/");
            var settings = CreateSettings();

            containerFactory.Create(new[] { bundle }, settings);

            externalBundle.WasProcessed.ShouldBeTrue();
        }

        protected CassetteSettings CreateSettings()
        {
            var settings = new CassetteSettings("")
            {
                SourceDirectory = Mock.Of<IDirectory>()
            };
            return settings;
        }
    }
}
