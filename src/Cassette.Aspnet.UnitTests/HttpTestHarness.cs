using System;
using System.Collections.Specialized;
using System.IO;
using System.Web;
using System.Web.Routing;
using Moq;

namespace Cassette
{
    class HttpTestHarness : IDisposable
    {
        public HttpTestHarness()
            : this(new RouteCollection())
        {
        }

        public HttpTestHarness(RouteCollection routes)
        {
            this.routes = routes;

            Context = new Mock<HttpContextBase>();
            Request = new Mock<HttpRequestBase>();
            Response = new Mock<HttpResponseBase>();
            RequestHeaders = new NameValueCollection();
            ResponseHeaders = new NameValueCollection();
            ResponseOutputStream = new MemoryStream();
            ResponseCache = new Mock<HttpCachePolicyBase>();
            Server = new Mock<HttpServerUtilityBase>();

            Context.SetupGet(c => c.Request).Returns(Request.Object);
            Context.SetupGet(c => c.Response).Returns(Response.Object);
            Context.SetupGet(c => c.Server).Returns(Server.Object);

            Request.SetupGet(r => r.PathInfo).Returns("");
            Request.SetupGet(r => r.Headers).Returns(RequestHeaders);

            Response.Setup(r => r.ApplyAppPathModifier(It.IsAny<string>())).Returns<string>(r => r);
            Response.SetupGet(r => r.OutputStream).Returns(ResponseOutputStream);
            Response.SetupGet(r => r.Cache).Returns(ResponseCache.Object);
            Response.SetupGet(r => r.Headers).Returns(ResponseHeaders);
        }

        RouteCollection routes;

        public Mock<HttpContextBase> Context;
        public Mock<HttpRequestBase> Request;
        public Mock<HttpResponseBase> Response;
        public Mock<HttpServerUtilityBase> Server;
        public NameValueCollection RequestHeaders;
        public NameValueCollection ResponseHeaders;
        public Mock<HttpCachePolicyBase> ResponseCache;
        public Stream ResponseOutputStream;

        class DelegateRouteHandler : IRouteHandler
        {
            readonly Func<RequestContext, IHttpHandler> createHandler;

            public DelegateRouteHandler(Func<RequestContext, IHttpHandler> createHandler)
            {
                this.createHandler = createHandler;
            }

            public IHttpHandler GetHttpHandler(RequestContext requestContext)
            {
                return createHandler(requestContext);
            }
        }

        public void Get(string url)
        {
            var queryStringStart = url.IndexOf('?');
            if (queryStringStart >= 0) url = url.Substring(0, queryStringStart);

            Request.SetupGet(r => r.RequestType).Returns("GET");
            Request.SetupGet(r => r.HttpMethod).Returns("GET");
            Request.SetupGet(r => r.AppRelativeCurrentExecutionFilePath).Returns(url);

            var routeData = routes.GetRouteData(Context.Object);
            if (routeData == null) throw new Exception("Route not found for URL: " + url);
            var httpHandler = routeData.RouteHandler.GetHttpHandler(new RequestContext(Context.Object, routeData));
            httpHandler.ProcessRequest(null);
            ResponseOutputStream.Position = 0;
        }

        public void Dispose()
        {
            ResponseOutputStream.Dispose();
        }
    }
}
