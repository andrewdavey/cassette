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
using System.Collections.Specialized;
using System.IO;
using System.IO.Compression;
using System.Web;
using System.Web.Routing;
using Cassette.Utilities;
using Moq;
using Should;
using Xunit;

namespace Cassette.Web
{
    public class ModuleRequestHandler_Tests : IDisposable
    {
        protected Mock<HttpContextBase> httpContext;
        protected Mock<HttpRequestBase> request;
        protected Mock<HttpResponseBase> response;
        protected Mock<HttpCachePolicyBase> responseCache;
        protected NameValueCollection requestHeaders;
        protected RouteData routeData;
        protected RequestContext requestContext;
        protected Stream outputStream;
        protected Mock<IModuleContainer<Module>> container;

        public ModuleRequestHandler_Tests()
        {
            httpContext = new Mock<HttpContextBase>();
            request = new Mock<HttpRequestBase>();
            requestHeaders = new NameValueCollection();
            response = new Mock<HttpResponseBase>();
            responseCache = new Mock<HttpCachePolicyBase>();
            routeData = new RouteData();
            requestContext = new RequestContext(httpContext.Object, routeData);
            outputStream = new MemoryStream();

            httpContext.SetupGet(c => c.Request).Returns(request.Object);
            httpContext.SetupGet(c => c.Response).Returns(response.Object);

            response.SetupGet(r => r.OutputStream).Returns(outputStream);
            response.SetupGet(r => r.Cache).Returns(responseCache.Object);

            request.SetupGet(r => r.Headers).Returns(requestHeaders);

            container = new Mock<IModuleContainer<Module>>();
        }

        protected ModuleRequestHandler<Module> CreateRequestHandler(string modulePath)
        {
            routeData.Values.Add("path", modulePath);
            return new ModuleRequestHandler<Module>(
                container.Object,
                requestContext
            );
        }

        protected void SetupTestModule()
        {
            var module = new Module("~/test");
            var asset = new Mock<IAsset>();
            asset.Setup(a => a.OpenStream())
                    .Returns(() => "asset-content".AsStream());
            asset.SetupGet(a => a.Hash).Returns(new byte[] { 1, 2, 3 });
            module.Assets.Add(asset.Object);
            container.Setup(c => c.FindModuleContainingPath("~\\test"))
                        .Returns(module);
        }

        void IDisposable.Dispose()
        {
            outputStream.Dispose();
        }
    }

    public class GivenModuleExists_WhenProcessRequest : ModuleRequestHandler_Tests
    {
        public GivenModuleExists_WhenProcessRequest()
        {
            var module = new Module("~/test") { ContentType = "expected-content-type" };
            var asset = new Mock<IAsset>();
            asset.Setup(a => a.OpenStream())
                    .Returns(() => "asset-content".AsStream());
            asset.SetupGet(a => a.Hash).Returns(new byte[] { 1, 2, 3 });
            module.Assets.Add(asset.Object);
            container.Setup(c => c.FindModuleContainingPath("~\\test"))
                        .Returns(module);

            var handler = CreateRequestHandler("test_010203");
            handler.ProcessRequest();
        }

        readonly DateTime start = DateTime.UtcNow;

        [Fact]
        public void ModuleAssetContentReturned()
        {
            outputStream.Position = 0;
            outputStream.ReadToEnd().ShouldEqual("asset-content");
        }

        [Fact]
        public void ContentTypeIsTheModuleContentType()
        {
            response.VerifySet(r => r.ContentType = "expected-content-type");
        }

        [Fact]
        public void ExpiresInOneYear()
        {
            responseCache.Verify(c => c.SetExpires(It.Is<DateTime>(d => (d - start).TotalDays >= 365)));
        }

        [Fact]
        public void ResponseIsPubliclyCacheable()
        {
            responseCache.Verify(c => c.SetCacheability(HttpCacheability.Public));
        }

        [Fact]
        public void ETagIsAssetHash()
        {
            var expectedETag = "\"010203\"";
            responseCache.Verify(c => c.SetETag(expectedETag));
        }
    }

    public class GivenModulePathIsMissingHashPostfix_WhenProcessRequest : ModuleRequestHandler_Tests
    {
        public GivenModulePathIsMissingHashPostfix_WhenProcessRequest()
        {
            SetupTestModule();

            var handler = CreateRequestHandler("test");
            handler.ProcessRequest();
        }

        [Fact]
        public void ModuleAssetContentReturned()
        {
            outputStream.Position = 0;
            outputStream.ReadToEnd().ShouldEqual("asset-content");
        }
    }

    public class GivenModuleDoesNotExist : ModuleRequestHandler_Tests
    {
        [Fact]
        public void HandlerReturns404()
        {
            var handler = CreateRequestHandler("scripts/unknown-module");

            handler.ProcessRequest();

            response.VerifySet(r => r.StatusCode = 404);
        }
    }

    public class GivenModuleExistsAndIfNonMatchHeaderIsEqualAssetHash_WhenProcessRequest : ModuleRequestHandler_Tests
    {
        public GivenModuleExistsAndIfNonMatchHeaderIsEqualAssetHash_WhenProcessRequest()
        {
            SetupTestModule();

            requestHeaders["If-None-Match"] = "\"010203\"";
            var handler = CreateRequestHandler("test");
            handler.ProcessRequest();
        }

        [Fact]
        public void StatusCodeIs304NotModified()
        {
            response.VerifySet(r => r.StatusCode = 304);
        }
    }

    public class GivenModuleExistsAndIfNonMatchHeaderIsNotEqualAssetHash_WhenProcessRequest : ModuleRequestHandler_Tests
    {
        public GivenModuleExistsAndIfNonMatchHeaderIsNotEqualAssetHash_WhenProcessRequest()
        {
            SetupTestModule();

            requestHeaders["If-None-Match"] = "xxxxxx";
            var handler = CreateRequestHandler("test");
            handler.ProcessRequest();
        }

        [Fact]
        public void ModuleAssetContentReturned()
        {
            outputStream.Position = 0;
            (outputStream.Length > 0).ShouldBeTrue();
        }
    }

    public class GivenRequestDeflateEncoding_WhenProcessRequest : ModuleRequestHandler_Tests
    {
        public GivenRequestDeflateEncoding_WhenProcessRequest()
        {
            requestHeaders.Add("Accept-Encoding", "deflate");
            response.SetupGet(r => r.Filter).Returns(Stream.Null);

            SetupTestModule();

            var handler = CreateRequestHandler("test");
            handler.ProcessRequest();
        }

        [Fact]
        public void ResponseFilterIsDeflateStream()
        {
            response.VerifySet(r => r.Filter = It.IsAny<DeflateStream>());
        }

        [Fact]
        public void ContentEncodingHeaderIsDeflate()
        {
            response.Verify(r => r.AppendHeader("Content-Encoding", "deflate"));
        }

        [Fact]
        public void VeryHeaderIsAcceptEncoding()
        {
            response.Verify(r => r.AppendHeader("Vary", "Accept-Encoding"));
        }
    }

    public class GivenRequestGZipEncoding_WhenProcessRequest : ModuleRequestHandler_Tests
    {
        public GivenRequestGZipEncoding_WhenProcessRequest()
        {
            requestHeaders.Add("Accept-Encoding", "gzip");
            response.SetupGet(r => r.Filter).Returns(Stream.Null);

            SetupTestModule();

            var handler = CreateRequestHandler("test");
            handler.ProcessRequest();
        }

        [Fact]
        public void ResponseFilterIsDeflateStream()
        {
            response.VerifySet(r => r.Filter = It.IsAny<GZipStream>());
        }

        [Fact]
        public void ContentEncodingHeaderIsDeflate()
        {
            response.Verify(r => r.AppendHeader("Content-Encoding", "gzip"));
        }

        [Fact]
        public void VeryHeaderIsAcceptEncoding()
        {
            response.Verify(r => r.AppendHeader("Vary", "Accept-Encoding"));
        }
    }

    public class GivenRequestWithUnrecognizedEncoding_WhenProcessRequest : ModuleRequestHandler_Tests
    {
        public GivenRequestWithUnrecognizedEncoding_WhenProcessRequest()
        {
            requestHeaders.Add("Accept-Encoding", "unknown");
            response.SetupGet(r => r.Filter).Returns(Stream.Null);

            SetupTestModule();

            var handler = CreateRequestHandler("test");
            handler.ProcessRequest();
        }

        [Fact]
        public void ResponseFilterIsNotSet()
        {
            response.VerifySet(r => r.Filter = It.IsAny<Stream>(), Times.Once()); // Only set once in the test constructor.
        }
    }

    public class ModuleRequestHandler_OcdTests : ModuleRequestHandler_Tests
    {
        [Fact]
        public void IsReusableIsFalse()
        {
            var handler = CreateRequestHandler("test");
            handler.IsReusable.ShouldBeFalse();
        }

        [Fact]
        public void IHttpHandlerIsExplicitlyImplemented()
        {
            var handler = CreateRequestHandler("test") as IHttpHandler;
            var context = new HttpContext(new HttpRequest("", "http://localhost/", ""), new HttpResponse(new StringWriter()));
            handler.ProcessRequest(context);
        }
    }
}