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
        protected readonly Mock<HttpContextBase> httpContext;
        protected readonly Dictionary<string, object> httpContextItems = new Dictionary<string, object>();

        public CassetteApplication_Tests()
        {
            sourceDir = new TempDirectory();
            cacheDir = new TempDirectory();
            httpContext = new Mock<HttpContextBase>();
            httpContext.SetupGet(c => c.Items).Returns(httpContextItems);
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
        public void WhenDispose_ThenBundleIsDisposed()
        {
            var bundle = new TestableBundle("~");
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
                new CassetteRouting(new VirtualDirectoryPrepender("/"), Mock.Of<IBundleContainer>), 
                () => httpContext.Object,
                ""
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

            application.OnPostMapRequestHandler();

            httpContextItems[typeof(IPlaceholderTracker).FullName].ShouldBeType<PlaceholderTracker>();
        }

        [Fact]
        public void GivenHtmlRewritingDisabled_WhenOnPostMapRequestHandler_ThenNullPlaceholderTrackerAddedToContextItems()
        {
            var application = StubApplication(settings => settings.IsHtmlRewritingEnabled = false);

            application.OnPostMapRequestHandler();

            httpContextItems[typeof(IPlaceholderTracker).FullName].ShouldBeType<NullPlaceholderTracker>();
        }
    }

    public class CassetteApplication_OnPostRequestHandlerExecute_Tests : CassetteApplication_Tests
    {
        [Fact]
        public void GivenHtmlRewritingDisabled_WhenOnPostRequestHandlerExecute_ThenResponseFilterIsNotSet()
        {
            var application = StubApplication(settings => settings.IsHtmlRewritingEnabled = false);

            var response = new Mock<HttpResponseBase>();
            httpContext.Setup(c => c.Response)
                       .Returns(response.Object);

            application.OnPostRequestHandlerExecute();

            response.VerifySet(r => r.Filter = It.IsAny<Stream>(), Times.Never());
        }

        [Fact]
        public void GivenCurrentHandlerIsAssemblyResourceLoader_WhenOnPostRequestHandlerExecute_ThenResponseFilterIsNotSet()
        {
            var application = StubApplication(settings => settings.IsHtmlRewritingEnabled = true);

            httpContext.SetupGet(c => c.CurrentHandler)
                       .Returns(new AssemblyResourceLoader());

            var response = new Mock<HttpResponseBase>();
            httpContext.Setup(c => c.Response)
                       .Returns(response.Object);

            application.OnPostRequestHandlerExecute();

            response.VerifySet(r => r.Filter = It.IsAny<Stream>(), Times.Never());
        }

        [Fact]
        public void GivenContentTypeIsNotHtml_WhenOnPostRequestHandlerExecute_ThenResponseFilterIsNotSet()
        {
            var application = StubApplication(settings => settings.IsHtmlRewritingEnabled = true);
            var response = new Mock<HttpResponseBase>();
            httpContext.Setup(c => c.Response).Returns(response.Object);
            response.SetupGet(r => r.ContentType).Returns("text/plain");

            application.OnPostRequestHandlerExecute();

            response.VerifySet(r => r.Filter = It.IsAny<Stream>(), Times.Never());
        }

        [Fact]
        public void GivenContentTypeisHtml_WhenOnPostRequestHandlerExecute_ThenPlaceholderReplacingResponseFilterIsInstalled()
        {
            var application = StubApplication(settings => settings.IsHtmlRewritingEnabled = true);
            var response = new Mock<HttpResponseBase>();
            var items = new Dictionary<string, object>();
            httpContext.Setup(c => c.Items).Returns(items);
            httpContext.Setup(c => c.Response).Returns(response.Object);
            response.SetupGet(r => r.ContentType).Returns("text/html");
            items[typeof(IPlaceholderTracker).FullName] = Mock.Of<IPlaceholderTracker>();

            application.OnPostRequestHandlerExecute();

            response.VerifySet(r => r.Filter = It.IsAny<PlaceholderReplacingResponseFilter>());
        }
    }
}