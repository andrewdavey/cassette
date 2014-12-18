﻿using System;
using System.Collections.Generic;
using System.Linq;
using Cassette.BundleProcessing;
using Cassette.IO;
using Cassette.Scripts;
using Cassette.Stylesheets;
using Moq;
using Should;
using Xunit;

namespace Cassette
{
    public abstract class ReferenceBuilder_Reference_TestBase
    {
        protected ReferenceBuilder_Reference_TestBase()
        {
            settings = new CassetteSettings();
            bundles = new BundleCollection(settings, Mock.Of<IFileSearchProvider>(), Mock.Of<IBundleFactoryProvider>(), Mock.Of<IBundleCollectionInitializer>());
            bundleFactoryProvider = new Mock<IBundleFactoryProvider>();
            placeholderTracker = new Mock<IPlaceholderTracker>();
            builder = new ReferenceBuilder(bundles, placeholderTracker.Object, bundleFactoryProvider.Object, settings);
        }

        internal readonly ReferenceBuilder builder;
        protected readonly BundleCollection bundles;
        protected readonly CassetteSettings settings;
        protected readonly Mock<IBundleFactoryProvider> bundleFactoryProvider;
        internal readonly Mock<IPlaceholderTracker> placeholderTracker;

        protected void AddBundles(params Bundle[] bundlesToAdd)
        {
            bundles.AddRange(bundlesToAdd);
            bundles.BuildReferences();
        }
    }

    public class ReferenceBuilder_Reference_Tests : ReferenceBuilder_Reference_TestBase
    {
        void SetBundleFactory<T>(Mock<IBundleFactory<T>> bundleFactory)
            where T : Bundle
        {
            bundleFactoryProvider
                .Setup(p => p.GetBundleFactory<T>())
                .Returns(bundleFactory.Object);
        }

        [Fact]
        public void WhenAddReferenceToBundleDirectory_ThenGetBundlesReturnTheBundle()
        {
            var bundle = new ScriptBundle("~/test");
            AddBundles(bundle);

            builder.Reference("test", null);

            builder.GetBundles(null).First().ShouldBeSameAs(bundle);
        }

        [Fact]
        public void WhenAddReferenceToSameBundleTwice_ThenGetBundlesReturnsOnlyOneBundle()
        {
            AddBundles(new ScriptBundle("~/test"));

            builder.Reference("test", null);
            builder.Reference("test", null);

            builder.GetBundles(null).Count().ShouldEqual(1);
        }

        [Fact]
        public void WhenAddReferenceToBundleDirectoryWithLocation_ThenGetBundlesThatLocationReturnTheBundle()
        {
            var bundle = new ScriptBundle("~/test");
            bundle.PageLocation = "body";
            AddBundles(bundle);

            builder.Reference("test", null);

            builder.GetBundles("body").First().ShouldBeSameAs(bundle);
        }

        [Fact]
        public void OnlyBundlesMatchingLocationAreReturnedByGetBundles()
        {
            var bundle1 = new ScriptBundle("~/test1");
            var bundle2 = new ScriptBundle("~/test2");
            bundle1.PageLocation = "body";
            AddBundles(bundle1, bundle2);

            builder.Reference("test1");
            builder.Reference("test2");

            var gotBundles = builder.GetBundles("body").ToArray();
            gotBundles.Length.ShouldEqual(1);
            gotBundles[0].ShouldBeSameAs(bundle1);
        }

        [Fact]
        public void WhenAddReferenceToNonExistentBundle_ThenThrowException()
        {
            Assert.Throws<ArgumentException>(delegate
            {
                builder.Reference("test");
            });
        }

        [Fact]
        public void GivenBundleAReferencesBundleB_WhenAddReferenceToBundleA_ThenGetBundlesReturnsBoth()
        {
            var bundleA = new ScriptBundle("~/a");
            var bundleB = new ScriptBundle("~/b");
            bundleA.AddReference("~/b");
            AddBundles(bundleA, bundleB);

            builder.Reference("a");

            builder.GetBundles(null).ShouldEqual(new[] { bundleB, bundleA });
        }

        [Fact]
        public void WhenAddReferenceToUnknownUrl_ThenGetBundlesReturnsAnExternalBundle()
        {
            bundles.BuildReferences();
            var bundleFactory = new Mock<IBundleFactory<ScriptBundle>>();
            bundleFactory.Setup(f => f.CreateBundle("http://test.com/test.js", It.IsAny<IEnumerable<IFile>>(), It.IsAny<BundleDescriptor>()))
                         .Returns(new ExternalScriptBundle("http://test.com/test.js") { Pipeline = StubPipeline<ScriptBundle>() });

            SetBundleFactory(bundleFactory);

            builder.Reference("http://test.com/test.js");

            var bundle = builder.GetBundles(null).First();
            bundle.ShouldBeType<ExternalScriptBundle>();
        }

        [Fact]
        public void WhenAddReferenceToUnknownUrl_ThenCreatedBundleIsProcessed()
        {
            bundles.BuildReferences();

            var bundleFactory = new Mock<IBundleFactory<ScriptBundle>>();
            var bundle = new ScriptBundle("~");
            var pipeline = new Mock<IBundlePipeline<ScriptBundle>>();
            bundle.Pipeline = pipeline.Object;
            bundleFactory.Setup(f => f.CreateBundle("http://test.com/test.js", It.IsAny<IEnumerable<IFile>>(), It.IsAny<BundleDescriptor>()))
                         .Returns(bundle);
            SetBundleFactory(bundleFactory);

            builder.Reference("http://test.com/test.js");

            pipeline.Verify(p => p.Process(bundle));
        }

        [Fact]
        public void WhenAddReferenceToUnknownHttpsUrl_ThenGetBundlesReturnsAnExternalBundle()
        {
            bundles.BuildReferences();
            var bundleFactory = new Mock<IBundleFactory<ScriptBundle>>();
            bundleFactory.Setup(f => f.CreateBundle("https://test.com/test.js", It.IsAny<IEnumerable<IFile>>(), It.IsAny<BundleDescriptor>()))
                         .Returns(new ExternalScriptBundle("https://test.com/test.js") { Pipeline = StubPipeline<ScriptBundle>() });
            SetBundleFactory(bundleFactory);

            builder.Reference("https://test.com/test.js");

            var bundle = builder.GetBundles(null).First();
            bundle.ShouldBeType<ExternalScriptBundle>();
        }

        [Fact]
        public void WhenAddReferenceToUnknownProtocolRelativeUrl_ThenGetBundlesReturnsAnExternalBundle()
        {
            bundles.BuildReferences();
            var bundleFactory = new Mock<IBundleFactory<ScriptBundle>>();
            bundleFactory.Setup(f => f.CreateBundle("//test.com/test.js", It.IsAny<IEnumerable<IFile>>(), It.IsAny<BundleDescriptor>()))
                         .Returns(new ExternalScriptBundle("//test.com/test.js") { Pipeline = StubPipeline<ScriptBundle>() });
            SetBundleFactory(bundleFactory);

            builder.Reference("//test.com/test.js");

            var bundle = builder.GetBundles(null).First();
            bundle.ShouldBeType<ExternalScriptBundle>();
        }

        [Fact]
        public void WhenAddReferenceToUnknownCssUrl_ThenExternalStylesheetBundleIsCreated()
        {
            bundles.BuildReferences();
            var bundleFactory = new Mock<IBundleFactory<StylesheetBundle>>();
            bundleFactory.Setup(f => f.CreateBundle("http://test.com/test.css", It.IsAny<IEnumerable<IFile>>(), It.IsAny<BundleDescriptor>()))
                         .Returns(new ExternalStylesheetBundle("http://test.com/test.css") { Pipeline = StubPipeline<StylesheetBundle>() });
            SetBundleFactory(bundleFactory);

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
        public void WhenAddReferenceToUrlWithJsFileExtensionAndQueryString_ThenGetBundlesReturnsAnExternalScriptBundle()
        {
            bundles.BuildReferences();
            var bundleFactory = new Mock<IBundleFactory<ScriptBundle>>();
            bundleFactory.Setup(f => f.CreateBundle("http://test.com/test.js?querystring", It.IsAny<IEnumerable<IFile>>(), It.IsAny<BundleDescriptor>()))
                         .Returns(new ExternalScriptBundle("http://test.com/test.js?querystring") { Pipeline = StubPipeline<ScriptBundle>() });

            SetBundleFactory(bundleFactory);

            builder.Reference("http://test.com/test.js?querystring");

            var bundle = builder.GetBundles(null).First();
            bundle.ShouldBeType<ExternalScriptBundle>();
        }

        [Fact]
        public void WhenAddReferenceToUnknownUrlWithBundleTypeAndUnexpectedExtension_ThenBundleCreatedInFactory()
        {
            bundles.BuildReferences();
            var bundleFactory = new Mock<IBundleFactory<StylesheetBundle>>();
            bundleFactory.Setup(f => f.CreateBundle("http://test.com/test", It.IsAny<IEnumerable<IFile>>(), It.IsAny<BundleDescriptor>()))
                         .Returns(new ExternalStylesheetBundle("http://test.com/test") { Pipeline = StubPipeline<StylesheetBundle>() });
            SetBundleFactory(bundleFactory);

            builder.Reference<StylesheetBundle>("http://test.com/test");

            builder.GetBundles(null).First().ShouldBeType<ExternalStylesheetBundle>();
        }
        
        [Fact]
        public void WhenAddReferenceWithLocation_ThenGetBundlesForThatLocationReturnsTheBundle()
        {
            var bundle = new ScriptBundle("~/test");
            AddBundles(bundle);

            builder.Reference("test", "body");

            builder.GetBundles("body").SequenceEqual(new[] { bundle}).ShouldBeTrue();
        }

        [Fact]
        public void GivenNullLocationAlreadyRendered_WhenAddReferenceToNullLocation_ThenExceptionThrown()
        {
            AddBundles(new ScriptBundle("~/test"));

            builder.Render<ScriptBundle>();

            Assert.Throws<InvalidOperationException>(
                () => builder.Reference("~/test")
            );
        }

        [Fact]
        public void GivenLocationAlreadyRendered_WhenAddReferenceToThatLocation_ThenExceptionThrown()
        {
            AddBundles(new ScriptBundle("~/test"));

            builder.Render<ScriptBundle>("location");

            Assert.Throws<InvalidOperationException>(
                () => builder.Reference("~/test", "location")
            );
        }

        [Fact]
        public void GivenLocationAlreadyRenderedButHtmlRewrittingEnabled_WhenAddReferenceToThatLocation_ThenBundleStillAdded()
        {
            settings.IsHtmlRewritingEnabled = true;
            var bundle = new ScriptBundle("~/test");
            AddBundles(bundle);

            builder.Render<ScriptBundle>("test");

            builder.Reference("~/test", "test");

            builder.GetBundles("test").First().ShouldBeSameAs(bundle);
        }

        [Fact]
        public void GivenTwoBundlesWithSamePathButDifferentType_WhenReferenceThePath_ThenBothBundlesAreReferenced()
        {
            var bundle1 = new ScriptBundle("~/test");
            var bundle2 = new StylesheetBundle("~/test");
            AddBundles(bundle1, bundle2);

            builder.Reference("~/test");
            builder.GetBundles(null).Count().ShouldEqual(2);
        }

        [Fact]
        public void GivenBundleReferencedInOneLocationAlsoUsedInAnother_WhenGetBundlesForSecondLocation_ThenBundleForFirstLocationIsNotIncluded()
        {
            var bundle1 = new TestableBundle("~/test1") { PageLocation = "head" };
            var bundle2 = new TestableBundle("~/test2");
            bundle2.AddReference("~/test1");
            AddBundles(bundle1, bundle2);

            builder.Reference("~/test2");
            builder.GetBundles(null).Count().ShouldEqual(1);
        }

        [Fact]
        public void GivenBundleReferencedInOneLocationAlsoUsedInAnotherAndPageLocationIsOverridden_WhenGetBundlesForSecondLocation_ThenBundleForFirstLocationIsNotIncluded()
        {
            var bundle1 = new TestableBundle("~/test1") { PageLocation = "head" };
            var bundle2 = new TestableBundle("~/test2");
            bundle2.AddReference("~/test1");
            AddBundles(bundle1, bundle2);

            builder.Reference("~/test2", "LOCATION");
            builder.GetBundles("LOCATION").Count().ShouldEqual(1);
        }

        [Fact]
        public void GivenBundlesWithNoPageLocationAssigned_WhenReferenceCallAssignsPageLocation_ThenGetBundlesHonoursTheNewAsignment()
        {
            var jquery = new TestableBundle("~/jquery");
            var app = new TestableBundle("~/app");
            app.AddReference("~/jquery");
            AddBundles(jquery, app);

            builder.Reference("~/jquery", "head");
            builder.Reference("~/app");

            builder.GetBundles("head").Single().ShouldBeSameAs(jquery);
            builder.GetBundles(null).Single().ShouldBeSameAs(app);
        }

        [Fact]
        public void GivenBundlesWithOnePageLocationAssigned_WhenReferenceCallOmitsPageLocation_ThenGetBundlesHonoursTheOriginalPageLocation()
        {
            var jquery = new TestableBundle("~/jquery") { PageLocation = "head" };
            var app = new TestableBundle("~/app");
            app.AddReference("~/jquery");
            AddBundles(jquery, app);
            
            builder.Reference("~/jquery");
            builder.Reference("~/app");

            builder.GetBundles("head").Single().ShouldBeSameAs(jquery);
            builder.GetBundles(null).Single().ShouldBeSameAs(app);
        }

        [Fact]
        public void ThreeBundlesWithDifferentPageLocationsInDependencyChainGetReferencedByOneReferenceCall()
        {
            var a = new TestableBundle("~/a") { PageLocation = "a" };
            var b = new TestableBundle("~/b") { PageLocation = "b" };
            var c = new TestableBundle("~/c");
            c.AddReference("~/b");
            b.AddReference("~/a");

            AddBundles(a, b, c);

            builder.Reference("~/c");

            builder.GetBundles(null).Single().ShouldBeSameAs(c);
            builder.GetBundles("a").Single().ShouldBeSameAs(a);
            builder.GetBundles("b").Single().ShouldBeSameAs(b);
        }

        [Fact]
        public void GivenBundleReferencedAtMultipleLevelsOfDependencyHierarchy_ThenGetBundlesReturnsItBeforeAllDependents()
        {
            var a = new TestableBundle("~/a");
            var b = new TestableBundle("~/b");
            var c = new TestableBundle("~/c");
            c.AddReference("~/b");
            c.AddReference("~/a");
            b.AddReference("~/a");

            AddBundles(a, b, c);

            builder.Reference("~/c");

            builder.GetBundles(null).ShouldEqual(new[] { a, b, c });
        }

        IBundlePipeline<T> StubPipeline<T>() where T : Bundle
        {
            return Mock.Of<IBundlePipeline<T>>();
        }
    }

    public class ReferenceBuilder_Render_Tests : ReferenceBuilder_Reference_TestBase
    {
        public ReferenceBuilder_Render_Tests()
        {
            placeholderTracker.Setup(t => t.InsertPlaceholder(It.IsAny<Func<string>>()))
                              .Returns(("output"));
        }

        [Fact]
        public void GivenAddReferenceToPath_WhenRender_ThenBundleRenderOutputReturned()
        {
            var bundle = new TestableBundle("~/test");
            AddBundles(bundle);

            builder.Reference("test");

            var html = builder.Render<TestableBundle>();

            html.ShouldEqual("output");
        }

        [Fact]
        public void GivenAddReferenceToPath_WhenRenderWithLocation_ThenBundleRenderOutputReturned()
        {
            var bundle = new TestableBundle("~/test") { RenderResult = "output" };
            AddBundles(bundle);

            builder.Reference("test");

            var html = builder.Render<TestableBundle>("body");

            html.ShouldEqual("output");
        }

        [Fact]
        public void GivenAddReferenceToTwoPaths_WhenRender_ThenBundleRenderOutputsSeparatedByNewLinesReturned()
        {
            var bundle1 = new TestableBundle("~/stub1") { RenderResult = "output1" };
            var bundle2 = new TestableBundle("~/stub2") { RenderResult = "output2" };
            AddBundles(bundle1, bundle2);

            builder.Reference("~/stub1");
            builder.Reference("~/stub2");

            Func<string> createHtml = null;
            placeholderTracker.Setup(t => t.InsertPlaceholder(It.IsAny<Func<string>>()))
                .Returns(("output"))
                .Callback<Func<string>>(f => createHtml = f);

            builder.Render<TestableBundle>();

            createHtml().ShouldEqual("output1" + Environment.NewLine + "output2");
        }
    }
}