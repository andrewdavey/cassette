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
using Cassette.Configuration;
using Cassette.IO;
using Cassette.Scripts;
using Cassette.Stylesheets;
using Moq;
using Should;
using Xunit;

namespace Cassette
{
    public class ReferenceBuilder_Reference_Tests
    {
        public ReferenceBuilder_Reference_Tests()
        {
            application = new Mock<ICassetteApplication>();
            bundleFactories = new Dictionary<Type, IBundleFactory<Bundle>>();
            bundleContainer = new Mock<IBundleContainer>();
            bundleContainer.Setup(c => c.IncludeReferencesAndSortBundles(It.IsAny<IEnumerable<Bundle>>()))
                           .Returns<IEnumerable<Bundle>>(ms => ms);

            builder = new ReferenceBuilder(bundleContainer.Object, bundleFactories, Mock.Of<IPlaceholderTracker>(), new CassetteSettings());
        }

        ReferenceBuilder builder;
        readonly Mock<IBundleContainer> bundleContainer;
        readonly Mock<ICassetteApplication> application;
        readonly Dictionary<Type, IBundleFactory<Bundle>> bundleFactories;

        [Fact]
        public void WhenAddReferenceToBundleDirectory_ThenGetBundlesReturnTheBundle()
        {
            var bundle = new ScriptBundle("~/test");
            bundleContainer.Setup(c => c.FindBundleContainingPath<Bundle>("~/test"))
                           .Returns(bundle);
            bundleContainer.Setup(c => c.IncludeReferencesAndSortBundles(It.IsAny<IEnumerable<Bundle>>()))
                           .Returns(new[] { bundle })
                           .Verifiable();

            builder.Reference("test", null);

            var bundles = builder.GetBundles(null).ToArray();
            bundles[0].ShouldBeSameAs(bundle);
            bundleContainer.Verify();
        }

        [Fact]
        public void WhenAddReferenceToSameBundleTwice_ThenGetBundlesReturnsOnlyOneBundle()
        {
            var bundle = new ScriptBundle("~/test");
            bundleContainer.Setup(c => c.FindBundleContainingPath<Bundle>("~/test"))
                           .Returns(bundle);
            bundleContainer.Setup(c => c.IncludeReferencesAndSortBundles(It.IsAny<IEnumerable<Bundle>>()))
                           .Returns(new[] { bundle })
                           .Verifiable();

            builder.Reference("test");
            builder.Reference("test");

            var bundles = builder.GetBundles(null).ToArray();
            bundles.Length.ShouldEqual(1);
        }

        [Fact]
        public void WhenAddReferenceToBundleDirectoryWithLocation_ThenGetBundlesThatLocationReturnTheBundle()
        {
            var bundle = new ScriptBundle("~/test");
            bundle.PageLocation = "body";
            bundleContainer.Setup(c => c.FindBundleContainingPath<Bundle>("~/test"))
                           .Returns(bundle);
            bundleContainer.Setup(c => c.IncludeReferencesAndSortBundles(It.IsAny<IEnumerable<Bundle>>()))
                           .Returns(new[] { bundle })
                           .Verifiable();
            builder.Reference("test", null);

            var bundles = builder.GetBundles("body").ToArray();

            bundles[0].ShouldBeSameAs(bundle);
            bundleContainer.Verify();
        }

        [Fact]
        public void OnlyBundlesMatchingLocationAreReturnedByGetBundles()
        {
            var bundle1 = new ScriptBundle("~/test1");
            var bundle2 = new ScriptBundle("~/test2");
            bundle1.PageLocation = "body";
            bundleContainer.Setup(c => c.FindBundleContainingPath<Bundle>("~/test1"))
                           .Returns(bundle1);
            bundleContainer.Setup(c => c.FindBundleContainingPath<Bundle>("~/test2"))
                           .Returns(bundle2);
            bundleContainer.Setup(c => c.IncludeReferencesAndSortBundles(It.IsAny<IEnumerable<Bundle>>()))
                           .Returns(new[] { bundle1 });
            builder.Reference("test1", null);
            builder.Reference("test2", null);

            var bundles = builder.GetBundles("body").ToArray();
            bundles.Length.ShouldEqual(1);
            bundles[0].ShouldBeSameAs(bundle1);
        }

        [Fact]
        public void WhenAddReferenceToNonExistentBundle_ThenThrowException()
        {
            bundleContainer.Setup(c => c.FindBundleContainingPath<Bundle>("~\\test")).Returns((ScriptBundle)null);

            Assert.Throws<ArgumentException>(delegate
            {
                builder.Reference("test", null);
            });
        }

        [Fact]
        public void GivenBundleAReferencesBundleB_WhenAddReferenceToBundleA_ThenGetBundlesReturnsBoth()
        {
            var bundleA = new ScriptBundle("~/a");
            var bundleB = new ScriptBundle("~/b");

            bundleContainer.Setup(c => c.FindBundleContainingPath<Bundle>("~/a"))
                           .Returns(bundleA);
            bundleContainer.Setup(c => c.IncludeReferencesAndSortBundles(It.IsAny<IEnumerable<Bundle>>()))
                           .Returns(new[] { bundleB, bundleA });

            builder.Reference("a", null);

            builder.GetBundles(null).SequenceEqual(new[] { bundleB, bundleA }).ShouldBeTrue();
        }

        [Fact]
        public void WhenAddReferenceToUnknownUrl_ThenGetBundlesReturnsAnExternalBundle()
        {
            var bundleFactory = new Mock<IBundleFactory<ScriptBundle>>();
            bundleFactory.Setup(f => f.CreateBundle("http://test.com/test.js", It.IsAny<IEnumerable<IFile>>(), It.IsAny<BundleDescriptor>()))
                         .Returns(new ExternalScriptBundle("http://test.com/test.js"));
            bundleFactories[typeof(ScriptBundle)] = bundleFactory.Object;

            builder.Reference("http://test.com/test.js", null);

            var bundle = builder.GetBundles(null).First();
            bundle.ShouldBeType<ExternalScriptBundle>();
        }

        [Fact]
        public void WhenAddReferenceToUnknownUrl_ThenCreatedBundleIsProcessed()
        {
            var bundleFactory = new Mock<IBundleFactory<TestableBundle>>();
            var bundle = new TestableBundle("~");
            bundleFactory.Setup(f => f.CreateBundle("http://test.com/test.js", It.IsAny<IEnumerable<IFile>>(), It.IsAny<BundleDescriptor>()))
                         .Returns(bundle);
            bundleFactories[typeof(ScriptBundle)] = bundleFactory.Object;

            builder.Reference("http://test.com/test.js", null);

            bundle.WasProcessed.ShouldBeTrue();
        }

        [Fact]
        public void WhenAddReferenceToUnknownHttpsUrl_ThenGetBundlesReturnsAnExternalBundle()
        {
            var bundleFactory = new Mock<IBundleFactory<ScriptBundle>>();
            bundleFactory.Setup(f => f.CreateBundle("https://test.com/test.js", It.IsAny<IEnumerable<IFile>>(), It.IsAny<BundleDescriptor>()))
                         .Returns(new ExternalScriptBundle("https://test.com/test.js"));
            bundleFactories[typeof(ScriptBundle)] = bundleFactory.Object;
            builder.Reference("https://test.com/test.js", null);

            var bundle = builder.GetBundles(null).First();
            bundle.ShouldBeType<ExternalScriptBundle>();
        }

        [Fact]
        public void WhenAddReferenceToUnknownProtocolRelativeUrl_ThenGetBundlesReturnsAnExternalBundle()
        {
            var bundleFactory = new Mock<IBundleFactory<ScriptBundle>>();
            bundleFactory.Setup(f => f.CreateBundle("//test.com/test.js", It.IsAny<IEnumerable<IFile>>(), It.IsAny<BundleDescriptor>()))
                         .Returns(new ExternalScriptBundle("//test.com/test.js"));
            bundleFactories[typeof(ScriptBundle)] = bundleFactory.Object;

            builder.Reference("//test.com/test.js");

            var bundle = builder.GetBundles(null).First();
            bundle.ShouldBeType<ExternalScriptBundle>();
        }

        [Fact]
        public void WhenAddReferenceToUnknownCssUrl_ThenExternalStylesheetBundleIsCreated()
        {
            var bundleFactory = new Mock<IBundleFactory<StylesheetBundle>>();
            bundleFactory.Setup(f => f.CreateBundle("http://test.com/test.css", It.IsAny<IEnumerable<IFile>>(), It.IsAny<BundleDescriptor>()))
                         .Returns(new ExternalStylesheetBundle("http://test.com/test.css"));
            bundleFactories[typeof(StylesheetBundle)] = bundleFactory.Object;

            builder.Reference("http://test.com/test.css");

            var bundle = builder.GetBundles(null).First();
            bundle.ShouldBeType<ExternalStylesheetBundle>();
        }

        [Fact]
        public void WhenAddReferenceToUrlWithUnexpectedExtension_ThenArgumentExceptionThrown()
        {
            Assert.Throws<ArgumentException>(
                () => builder.Reference("http://test.com/test")
            );
        }

        [Fact]
        public void WhenAddReferenceToUnknownUrlWithBundleTypeAndUnexpectedExtension_ThenBundleCreatedInFactory()
        {
            var bundleFactory = new Mock<IBundleFactory<StylesheetBundle>>();
            bundleFactory.Setup(f => f.CreateBundle("http://test.com/test", It.IsAny<IEnumerable<IFile>>(), It.IsAny<BundleDescriptor>()))
                         .Returns(new ExternalStylesheetBundle("http://test.com/test"));
            bundleFactories[typeof(StylesheetBundle)] = bundleFactory.Object;

            builder.Reference<StylesheetBundle>("http://test.com/test");

            builder.GetBundles(null).First().ShouldBeType<ExternalStylesheetBundle>();
        }
        
        [Fact]
        public void WhenAddReferenceWithLocation_ThenGetBundlesForThatLocationReturnsTheBundle()
        {
            var bundle = new ScriptBundle("~/test");
            bundleContainer.Setup(c => c.FindBundleContainingPath<Bundle>("~/test"))
                           .Returns(bundle);
            bundleContainer.Setup(c => c.IncludeReferencesAndSortBundles(It.IsAny<IEnumerable<Bundle>>()))
                           .Returns(new[] { bundle });
            builder.Reference("test", "body");

            builder.GetBundles("body").SequenceEqual(new[] { bundle}).ShouldBeTrue();
        }

        [Fact]
        public void GivenNullLocationAlreadyRendered_WhenAddReferenceToNullLocation_ThenExceptionThrown()
        {
            var bundle = new ScriptBundle("~/test");
            bundleContainer.Setup(c => c.FindBundleContainingPath<Bundle>("~/test"))
                           .Returns(bundle);

            builder.Render<ScriptBundle>();

            Assert.Throws<InvalidOperationException>(
                () => builder.Reference("~/test")
            );
        }

        [Fact]
        public void GivenLocationAlreadyRendered_WhenAddReferenceToThatLocation_ThenExceptionThrown()
        {
            var bundle = new ScriptBundle("~/test");
            bundleContainer.Setup(c => c.FindBundleContainingPath<Bundle>("~/test"))
                           .Returns(bundle);

            builder.Render<ScriptBundle>("location");

            Assert.Throws<InvalidOperationException>(
                () => builder.Reference("~/test", "location")
            );
        }

        [Fact]
        public void GivenLocationAlreadyRenderedButHtmlRewrittingEnabled_WhenAddReferenceToThatLocation_ThenBundleStillAdded()
        {
            builder = new ReferenceBuilder(
                bundleContainer.Object,
                bundleFactories, Mock.Of<IPlaceholderTracker>(), 
                new CassetteSettings { IsHtmlRewritingEnabled = true }
            );
            var bundle = new ScriptBundle("~/test");
            bundleContainer.Setup(c => c.FindBundleContainingPath<Bundle>("~/test"))
                           .Returns(bundle);
            builder.Render<ScriptBundle>("test");

            builder.Reference("~/test", "test");

            builder.GetBundles("test").First().ShouldBeSameAs(bundle);
        }
    }

    public class ReferenceBuilder_Render_Tests
    {
        public ReferenceBuilder_Render_Tests()
        {
            bundleContainer = new Mock<IBundleContainer>();
            placeholderTracker = new Mock<IPlaceholderTracker>();
            application = Mock.Of<ICassetteApplication>();
            bundleFactories = new Dictionary<Type, IBundleFactory<Bundle>>();

            bundleContainer.Setup(c => c.IncludeReferencesAndSortBundles(It.IsAny<IEnumerable<Bundle>>()))
                           .Returns<IEnumerable<Bundle>>(ms => ms);

            placeholderTracker.Setup(t => t.InsertPlaceholder(It.IsAny<Func<string>>()))
                              .Returns(("output"));

            referenceBuilder = new ReferenceBuilder(bundleContainer.Object, bundleFactories, placeholderTracker.Object, new CassetteSettings());
        }

        readonly ReferenceBuilder referenceBuilder;
        readonly Mock<IPlaceholderTracker> placeholderTracker;
        readonly ICassetteApplication application;
        readonly Mock<IBundleContainer> bundleContainer;
        readonly Dictionary<Type, IBundleFactory<Bundle>> bundleFactories;

        [Fact]
        public void GivenAddReferenceToPath_WhenRender_ThenBundleRenderOutputReturned()
        {
            var bundle = new TestableBundle("~/stub");
            bundleContainer.Setup(c => c.FindBundleContainingPath<Bundle>(It.IsAny<string>()))
                           .Returns(bundle);

            referenceBuilder.Reference("test");

            var html = referenceBuilder.Render<TestableBundle>();

            html.ShouldEqual("output");
        }

        [Fact]
        public void GivenAddReferenceToPath_WhenRenderWithLocation_ThenBundleRenderOutputReturned()
        {
            var bundle = new TestableBundle("~/stub") { RenderResult = "output" };
            bundleContainer.Setup(c => c.FindBundleContainingPath<Bundle>(It.IsAny<string>()))
                           .Returns(bundle);
            referenceBuilder.Reference("test");

            var html = referenceBuilder.Render<TestableBundle>("body");

            html.ShouldEqual("output");
        }

        [Fact]
        public void GivenAddReferenceToTwoPaths_WhenRender_ThenBundleRenderOutputsSeparatedByNewLinesReturned()
        {
            var bundle1 = new TestableBundle("~/stub1") { RenderResult = "output1" };
            var bundle2 = new TestableBundle("~/stub2") { RenderResult = "output2" };
            bundleContainer.Setup(c => c.FindBundleContainingPath<Bundle>("~/stub1"))
                           .Returns(bundle1);
            bundleContainer.Setup(c => c.FindBundleContainingPath<Bundle>("~/stub2"))
                           .Returns(bundle2);

            referenceBuilder.Reference("~/stub1");
            referenceBuilder.Reference("~/stub2");

            Func<string> createHtml = null;
            placeholderTracker.Setup(t => t.InsertPlaceholder(It.IsAny<Func<string>>()))
                .Returns(("output"))
                .Callback<Func<string>>(f => createHtml = f);

            referenceBuilder.Render<TestableBundle>();

            createHtml().ShouldEqual("output1" + Environment.NewLine + "output2");
        }
    }
}

