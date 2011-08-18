using System;
using System.Collections.Specialized;
using System.IO;
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

        public class GivenModuleExists_WhenProcessRequest : ModuleRequestHandler_Tests
        {
            public GivenModuleExists_WhenProcessRequest()
            {
                var module = new Module("test") { ContentType = "expected-content-type" };
                var asset = new Mock<IAsset>();
                asset.Setup(a => a.OpenStream())
                     .Returns(() => "asset-content".AsStream());
                asset.SetupGet(a => a.Hash).Returns(new byte[] { 1, 2, 3 });
                module.Assets.Add(asset.Object);
                container.Setup(c => c.FindModuleContainingPath("test"))
                         .Returns(module);
                
                var handler = CreateRequestHandler("test");
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
                var expectedETag = "010203";
                responseCache.Verify(c => c.SetETag(expectedETag));
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
                var module = new Module("test");
                var asset = new Mock<IAsset>();
                asset.Setup(a => a.OpenStream())
                     .Returns(() => "asset-content".AsStream());
                asset.SetupGet(a => a.Hash).Returns(new byte[] { 1, 2, 3 });
                module.Assets.Add(asset.Object);
                container.Setup(c => c.FindModuleContainingPath("test"))
                         .Returns(module);

                requestHeaders["If-None-Match"] = "010203";
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
                var module = new Module("test");
                var asset = new Mock<IAsset>();
                asset.Setup(a => a.OpenStream())
                     .Returns(() => "asset-content".AsStream());
                asset.SetupGet(a => a.Hash).Returns(new byte[] { 1, 2, 3 });
                module.Assets.Add(asset.Object);
                container.Setup(c => c.FindModuleContainingPath("test"))
                         .Returns(module);

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

        void IDisposable.Dispose()
        {
            outputStream.Dispose();
        }
    }
}
