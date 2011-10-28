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
            using (CreateApplication(bundles => bundles.AddPerSubDirectory<ScriptBundle>("Scripts")))
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
            using (CreateApplication(bundles => bundles.AddPerSubDirectory<ScriptBundle>("Scripts")))
            {
                using (var http = new HttpTestHarness(routes))
                {
                    http.Get("~/_cassette/scriptbundle/scripts/bundle-b");
                    http.ResponseOutputStream.ReadToEnd().ShouldEqual("function asset3(){}");
                }
            }
        }

        [Fact]
        public void CanGetAsset()
        {
            using (CreateApplication(bundles => bundles.AddPerSubDirectory<ScriptBundle>("Scripts")))
            {
                using (var http = new HttpTestHarness(routes))
                {
                    http.Get("~/_cassette/asset/scripts/bundle-a/asset-1.js?123");
                    http.ResponseOutputStream.ReadToEnd().ShouldEqual(@"/// <reference path=""asset-2.js""/>

function asset1() {
}");
                }
            }
        }

        [Fact]
        public void GetCoffeeScriptAssetReturnsItCompiledInToJavaScript()
        {
            using (CreateApplication(bundles => bundles.AddPerSubDirectory<ScriptBundle>("Scripts")))
            {
                using (var http = new HttpTestHarness(routes))
                {
                    http.Get("~/_cassette/asset/scripts/bundle-c/asset.coffee?123");
                    http.ResponseOutputStream.ReadToEnd().ShouldEqual(@"(function() {
  var x;
  x = 1;
}).call(this);
".Replace("\r\n", "\n"));
                }
            }
            
        }

        CassetteApplication CreateApplication(Action<BundleCollection> configure)
        {
            var settings = new CassetteSettings
            {
                CacheDirectory = new IsolatedStorageDirectory(storage),
                SourceDirectory = new FileSystemDirectory(Path.GetFullPath("assets"))
            };
            var bundles = new BundleCollection(settings);
            new BundleInitializerConfiguration().Configure(bundles, settings);
            configure(bundles);
            return new CassetteApplication(
                bundles,
                settings,
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
            var queryStringStart = url.IndexOf('?');
            if (queryStringStart >= 0) url = url.Substring(0, queryStringStart);

            Request.SetupGet(r => r.RequestType).Returns("GET");
            Request.SetupGet(r => r.HttpMethod).Returns("GET");
            Request.SetupGet(r => r.AppRelativeCurrentExecutionFilePath)
                   .Returns(url);

            var routeData = routes.GetRouteData(Context.Object);
            if (routeData == null) return;
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
