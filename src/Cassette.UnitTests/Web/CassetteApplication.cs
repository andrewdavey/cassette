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
using Cassette.UI;
using Moq;
using Should;
using Xunit;

namespace Cassette.Web
{
    public class CassetteApplication_Tests : IDisposable
    {
        readonly TempDirectory cacheDir;
        readonly TempDirectory sourceDir;

        protected CassetteApplication_Tests()
        {
            sourceDir = new TempDirectory();
            cacheDir = new TempDirectory();
        }

        internal CassetteApplication StubApplication(bool isHtmlRewritingEnabled = false)
        {
            var settings = new CassetteSettings
            {
                IsHtmlRewritingEnabled = isHtmlRewritingEnabled,
                CacheDirectory = new FileSystemDirectory(cacheDir),
                SourceDirectory = new FileSystemDirectory(sourceDir)
            };
            return new CassetteApplication(
                new BundleCollection(settings), 
                settings,
                new CassetteRouting(new VirtualDirectoryPrepender("/")), 
                new RouteCollection(), 
                () => null
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
            var application = StubApplication(isHtmlRewritingEnabled: true);

            var context = new Mock<HttpContextBase>();
            var items = new Dictionary<string, object>();
            context.SetupGet(c => c.Items).Returns(items);

            application.OnPostMapRequestHandler(context.Object);

            items[typeof(IPlaceholderTracker).FullName].ShouldBeType<PlaceholderTracker>();
        }

        [Fact]
        public void GivenHtmlRewritingDisabled_WhenOnPostMapRequestHandler_ThenNullPlaceholderTrackerAddedToContextItems()
        {
            var application = StubApplication(isHtmlRewritingEnabled: false);

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
            var application = StubApplication(isHtmlRewritingEnabled: false);

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
            var application = StubApplication(isHtmlRewritingEnabled: true);

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

