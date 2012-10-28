using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.IO;
using System.Web;
using Cassette.Aspnet;
using Moq;

namespace Cassette
{
    class HttpTestHarness : IDisposable
    {
        readonly WebHost host;

        public HttpTestHarness(WebHost host)
        {
            this.host = host;
            Context = new Mock<HttpContextBase>();
            Request = new Mock<HttpRequestBase>();
            Response = new Mock<HttpResponseBase>();
            RequestHeaders = new NameValueCollection();
            ResponseOutputStream = new MemoryStream();
            ResponseCache = new Mock<HttpCachePolicyBase>();

            Context.SetupGet(c => c.Request).Returns(Request.Object);
            Context.SetupGet(c => c.Response).Returns(Response.Object);
            Context.SetupGet(c => c.Items).Returns(new Dictionary<string, object>());

            Request.SetupGet(r => r.PathInfo).Returns("");
            Request.SetupGet(r => r.Headers).Returns(RequestHeaders);

            Response.Setup(r => r.ApplyAppPathModifier(It.IsAny<string>())).Returns<string>(r => r);
            Response.SetupGet(r => r.OutputStream).Returns(ResponseOutputStream);
            Response.SetupGet(r => r.Cache).Returns(ResponseCache.Object);
            Response.SetupProperty(r => r.ContentType);
            Response.Setup(r => r.Write(It.IsAny<string>()))
                    .Callback<string>(content => WrittenBody += content);
        }

        public Mock<HttpContextBase> Context;
        public Mock<HttpRequestBase> Request;
        public Mock<HttpResponseBase> Response;
        public NameValueCollection RequestHeaders;
        public Mock<HttpCachePolicyBase> ResponseCache;
        public Stream ResponseOutputStream;
        public string WrittenBody = "";

        public void Get(string url)
        {
            if (!url.StartsWith("/cassette.axd")) throw new ArgumentException("Must be a Cassette handler URL.", "url");

            if (url.Contains("?")) url = url.Substring(0, url.IndexOf('?'));

            var pathInfo = url.Substring("/cassette.axd".Length);

            Request.SetupGet(r => r.RequestType).Returns("GET");
            Request.SetupGet(r => r.HttpMethod).Returns("GET");
            Request.SetupGet(r => r.AppRelativeCurrentExecutionFilePath).Returns("~/cassette.axd");
            Request.SetupGet(r => r.PathInfo).Returns(pathInfo);

            host.StoreRequestContainerInHttpContextItems();
            var httpHandler = new CassetteHttpHandler(host.RequestContainer, Request.Object);
            httpHandler.ProcessRequest();
            ResponseOutputStream.Position = 0;
            host.RemoveRequestContainerFromHttpContextItems();
        }

        public void Dispose()
        {
            ResponseOutputStream.Dispose();
        }
    }
}