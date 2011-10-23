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
using System.IO;
using System.Web;
using System.Web.Handlers;
using System.Web.Routing;
using Cassette.Configuration;
using Cassette.IO;
using Moq;
using Should;
using Xunit;

namespace Cassette.Web
{
    public class CassetteApplication_Tests : IDisposable
    {
        readonly TempDirectory cacheDir;
        readonly TempDirectory sourceDir;
        readonly Mock<HttpContextBase> httpContext;

        public CassetteApplication_Tests()
        {
            sourceDir = new TempDirectory();
            cacheDir = new TempDirectory();
            httpContext = new Mock<HttpContextBase>();
        }

        [Fact]
        public void IsDebuggingEnabledPropertyIsAssignedFromSettings()
        {
            var application = StubApplication(settings => settings.IsDebuggingEnabled = true);
            application.IsDebuggingEnabled.ShouldBeTrue();
        }

        [Fact]
        public void SourceDirectoryPropertyIsAssignedFromSettings()
        {
            var directory = Mock.Of<IDirectory>();
            var application = StubApplication(settings => settings.SourceDirectory = directory);
            application.SourceDirectory.ShouldBeSameAs(directory);
        }

        [Fact]
        public void IsHtmlRewritingEnabledPropertyIsAssignedFromSettings()
        {
            var application = StubApplication(settings => settings.IsHtmlRewritingEnabled = true);
            application.IsHtmlRewritingEnabled.ShouldBeTrue();
        }

        [Fact]
        public void UrlGeneratorIsAssigned()
        {
            var application = StubApplication();
            application.UrlGenerator.ShouldNotBeNull();
        }

        [Fact]
        public void WhenGetReferenceBuilder_ThenReferenceBuilderObjectIsStoredInHttpContextItems()
        {
            var items = new Dictionary<string, object>();
            httpContext.SetupGet(c => c.Items).Returns(items);

            var application = StubApplication();
            var builder = application.GetReferenceBuilder();
            
            builder.ShouldNotBeNull();
            var storedBuilder = items["Cassette.ReferenceBuilder"];
            storedBuilder.ShouldBeSameAs(builder);
        }

        [Fact]
        public void GivenReferenceBuilderCreatedOnce_WhenGetReferenceBuilderAgain_ThenTheSameObjectIsReturned()
        {
            var items = new Dictionary<string, object>();
            httpContext.SetupGet(c => c.Items).Returns(items);
            var application = StubApplication();
            var builder = application.GetReferenceBuilder();

            var builder2 = application.GetReferenceBuilder();
            builder2.ShouldBeSameAs(builder);
        }

        [Fact]
        public void GivenSettingsHasDefaultInitializerForBundleType_WhenGetDefaultBundleInitializer_ThenInitializerReturnedForBundleType()
        {
            var initializer = Mock.Of<IBundleInitializer>();
            var application = StubApplication(
                settings => settings.DefaultBundleInitializers[typeof(TestableBundle)] = initializer
            );

            var actualInitializer = application.GetDefaultBundleInitializer(typeof(TestableBundle));

            actualInitializer.ShouldBeSameAs(initializer);
        }

        [Fact]
        public void GivenSettingsHasDefaultInitializerForScriptBundleType_WhenGetDefaultBundleInitializerForExternalScriptBundleType_ThenInitializerReturned()
        {
            var initializer = Mock.Of<IBundleInitializer>();
            var application = StubApplication(
                settings => settings.DefaultBundleInitializers[typeof(Scripts.ScriptBundle)] = initializer
            );

            var actualInitializer = application.GetDefaultBundleInitializer(typeof(Scripts.ExternalScriptBundle));

            actualInitializer.ShouldBeSameAs(initializer);
        }

        [Fact]
        public void GivenSettingsHasDefaultInitializerForStylesheetBundleType_WhenGetDefaultBundleInitializerForExternalStylesheetBundleType_ThenInitializerReturned()
        {
            var initializer = Mock.Of<IBundleInitializer>();
            var application = StubApplication(
                settings => settings.DefaultBundleInitializers[typeof(Stylesheets.StylesheetBundle)] = initializer
            );

            var actualInitializer = application.GetDefaultBundleInitializer(typeof(Stylesheets.ExternalStylesheetBundle));

            actualInitializer.ShouldBeSameAs(initializer);
        }

        [Fact]
        public void GivenNoBundleInitializerRegistered_WhenGetDefaultBundleInitializer_ThenArgumentExceptionIsThrown()
        {
            var application = StubApplication();

            Assert.Throws<ArgumentException>(
                () => application.GetDefaultBundleInitializer(typeof(Bundle))
            );
        }

        [Fact]
        public void WhenDispose_ThenBundleIsDisposed()
        {
            var bundle = new TestableBundle("~", false);
            var application = StubApplication(createBundles: settings => new BundleCollection(settings) { bundle });
            
            application.Dispose();

            bundle.WasDisposed.ShouldBeTrue();
        }

        internal CassetteApplication StubApplication(Action<CassetteSettings> alterSettings = null, Func<CassetteSettings, BundleCollection> createBundles = null)
        {
            var settings = new CassetteSettings
            {
                CacheDirectory = new FileSystemDirectory(cacheDir),
                SourceDirectory = new FileSystemDirectory(sourceDir)
            };
            if (alterSettings != null) alterSettings(settings);

            var bundles = createBundles == null 
                ? new BundleCollection(settings) 
                : createBundles(settings);

            return new CassetteApplication(
                bundles, 
                settings,
                new CassetteRouting(new VirtualDirectoryPrepender("/")), 
                new RouteCollection(), 
                () => httpContext.Object
            );
        }

        public void Dispose()
        {
           cacheDir.Dispose();
           sourceDir.Dispose();
        }
    }

    public class CassetteApplication_OnPostMapRequestHandler_Tests : CassetteApplication_Tests
    {
        [Fact]
        public void GivenHtmlRewritingEnabled_WhenOnPostMapRequestHandler_ThenPlaceholderTrackerAddedToContextItems()
        {
            var application = StubApplication(settings => settings.IsHtmlRewritingEnabled = true);

            var context = new Mock<HttpContextBase>();
            var items = new Dictionary<string, object>();
            context.SetupGet(c => c.Items).Returns(items);

            application.OnPostMapRequestHandler(context.Object);

            items[typeof(IPlaceholderTracker).FullName].ShouldBeType<PlaceholderTracker>();
        }

        [Fact]
        public void GivenHtmlRewritingDisabled_WhenOnPostMapRequestHandler_ThenNullPlaceholderTrackerAddedToContextItems()
        {
            var application = StubApplication(settings => settings.IsHtmlRewritingEnabled = false);

            var context = new Mock<HttpContextBase>();
            var items = new Dictionary<string, object>();
            context.SetupGet(c => c.Items).Returns(items);

            application.OnPostMapRequestHandler(context.Object);

            items[typeof(IPlaceholderTracker).FullName].ShouldBeType<NullPlaceholderTracker>();
        }
    }

    public class CassetteApplication_OnPostRequestHandlerExecute_Tests : CassetteApplication_Tests
    {
        [Fact]
        public void GivenHtmlRewritingDisabled_WhenOnPostRequestHandlerExecute_ThenResponseFilterIsNotSet()
        {
            var application = StubApplication(settings => settings.IsHtmlRewritingEnabled = false);

            var context = new Mock<HttpContextBase>();
            var response = new Mock<HttpResponseBase>();
            context.Setup(c => c.Response)
                   .Returns(response.Object);

            application.OnPostRequestHandlerExecute(context.Object);

            response.VerifySet(r => r.Filter = It.IsAny<Stream>(), Times.Never());
        }

        [Fact]
        public void GivenCurrentHandlerIsAssemblyResourceLoader_WhenOnPostRequestHandlerExecute_ThenResponseFilterIsNotSet()
        {
            var application = StubApplication(settings => settings.IsHtmlRewritingEnabled = true);

            var context = new Mock<HttpContextBase>();
            context.SetupGet(c => c.CurrentHandler)
                   .Returns(new AssemblyResourceLoader());

            var response = new Mock<HttpResponseBase>();
            context.Setup(c => c.Response)
                   .Returns(response.Object);

            application.OnPostRequestHandlerExecute(context.Object);

            response.VerifySet(r => r.Filter = It.IsAny<Stream>(), Times.Never());
        }
    }
}