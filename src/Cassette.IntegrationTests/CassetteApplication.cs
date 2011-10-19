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
using System.IO.IsolatedStorage;
using System.Web;
using System.Web.Routing;
using Cassette.Configuration;
using Cassette.IO;
using Cassette.Scripts;
using Cassette.Web;
using Moq;
using Should;
using Xunit;

namespace Cassette.IntegrationTests
{
    public class CassetteApplicationTests : IDisposable
    {
        public CassetteApplicationTests()
        {
            RemoveExistingCache();

            storage = IsolatedStorageFile.GetMachineStoreForAssembly();
            routes = new RouteCollection();
        }

        readonly IsolatedStorageFile storage;
        readonly RouteCollection routes;

        [Fact]
        public void CanGetScriptBundleA()
        {
            using (CreateApplication(bundles => bundles.AddForEachSubDirectory<ScriptBundle>("Scripts")))
            {
                using (var http = new HttpTestHarness(routes))
                {
                    http.Get("~/_cassette/scriptbundle/scripts/bundle-a");
                    http.ResponseOutputStream.ReadToEnd().ShouldEqual("function asset2(){}function asset1(){}");
                }
            }
        }

        [Fact]
        public void CanGetScriptBundleB()
        {
            using (CreateApplication(bundles => bundles.AddForEachSubDirectory<ScriptBundle>("Scripts")))
            {
                using (var http = new HttpTestHarness(routes))
                {
                    http.Get("~/_cassette/scriptbundle/scripts/bundle-b");
                    http.ResponseOutputStream.ReadToEnd().ShouldEqual("function asset3(){}");
                }
            }
        }

        CassetteApplication CreateApplication(Action<BundleCollection> configure)
        {
            var config = new ConfigurableCassetteApplication
            {
                Settings =
                {
                    CacheDirectory = new IsolatedStorageDirectory(storage),
                    SourceDirectory = new FileSystemDirectory(Path.GetFullPath("../../assets"))
                }
            };
            configure(config.Bundles);
            return new CassetteApplication(
                config,
                "",
                new CassetteRouting(new VirtualDirectoryPrepender("/")),
                routes,
                Mock.Of<HttpContextBase>
            );
        }

        void RemoveExistingCache()
        {
            using (var storage = IsolatedStorageFile.GetMachineStoreForAssembly())
            {
                storage.Remove();
            }
        }

        void IDisposable.Dispose()
        {
            storage.Dispose();
        }
    }

    class HttpTestHarness : IDisposable
    {
        public HttpTestHarness(RouteCollection routes)
        {
            this.routes = routes;

            Context = new Mock<HttpContextBase>();
            Request = new Mock<HttpRequestBase>();
            Response = new Mock<HttpResponseBase>();
            RequestHeaders = new NameValueCollection();
            ResponseOutputStream = new MemoryStream();
            ResponseCache = new Mock<HttpCachePolicyBase>();

            Context.SetupGet(c => c.Request)
                   .Returns(Request.Object);
            Context.SetupGet(c => c.Response)
                   .Returns(Response.Object);

            Request.SetupGet(r => r.PathInfo).Returns("");
            Request.SetupGet(r => r.Headers).Returns(RequestHeaders);

            Response.Setup(r => r.ApplyAppPathModifier(It.IsAny<string>())).Returns<string>(r => r);
            Response.SetupGet(r => r.OutputStream).Returns(ResponseOutputStream);
            Response.SetupGet(r => r.Cache).Returns(ResponseCache.Object);
        }

        RouteCollection routes;

        public Mock<HttpContextBase> Context;
        public Mock<HttpRequestBase> Request;
        public Mock<HttpResponseBase> Response;
        public NameValueCollection RequestHeaders;
        public Mock<HttpCachePolicyBase> ResponseCache;
        public Stream ResponseOutputStream;

        public void Get(string url)
        {
            Request.SetupGet(r => r.RequestType).Returns("GET");
            Request.SetupGet(r => r.HttpMethod).Returns("GET");
            Request.SetupGet(r => r.AppRelativeCurrentExecutionFilePath)
                   .Returns(url);

            var routeData = routes.GetRouteData(Context.Object);
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
