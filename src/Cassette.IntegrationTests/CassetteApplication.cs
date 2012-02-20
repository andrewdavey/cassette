﻿using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.IO;
using System.Web;
using System.Web.Routing;
using Cassette.Configuration;
using Cassette.HtmlTemplates;
using Cassette.IO;
using Cassette.Scripts;
using Cassette.Stylesheets;
using Cassette.Web;
using Moq;
using Should;
using Xunit;
using IsolatedStorageFile = System.IO.IsolatedStorage.IsolatedStorageFile;

namespace Cassette.IntegrationTests
{
    public class CassetteApplicationTests : IDisposable
    {
        public CassetteApplicationTests()
        {
            RemoveExistingCache();

            storage = IsolatedStorageFile.GetMachineStoreForDomain();
            routes = new RouteCollection();
            httpContext = new Mock<HttpContextBase>();
            httpContextItems = new Dictionary<string, object>();
            httpContext.Setup(c => c.Items).Returns(httpContextItems);
        }

        readonly IsolatedStorageFile storage;
        readonly RouteCollection routes;
        readonly Mock<HttpContextBase> httpContext;
        readonly Dictionary<string, object> httpContextItems;

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
        public void GivenDebugMode_ThenCanGetAsset()
        {
            using (CreateApplication(bundles => bundles.AddPerSubDirectory<ScriptBundle>("Scripts"), isDebuggingEnabled: true))
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
        public void GivenDebugMode_ThenGetCoffeeScriptAssetReturnsItCompiledInToJavaScript()
        {
            using (CreateApplication(bundles => bundles.AddPerSubDirectory<ScriptBundle>("Scripts"), isDebuggingEnabled: true))
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

        [Fact]
        public void HtmlTemplatesCanBeRenderedInlineInPage()
        {
            using (var app = CreateApplication(bundles => bundles.Add<HtmlTemplateBundle>("templates")))
            {
                app.OnPostMapRequestHandler();

                var referenceBuilder = app.GetReferenceBuilder();
                referenceBuilder.Reference("templates");
                var html = referenceBuilder.Render<HtmlTemplateBundle>(null);
                html.ShouldEqual(
                    "<script id=\"asset-1\" type=\"text/html\"><p>asset 1</p></script>" +
                    Environment.NewLine + 
                    "<script id=\"asset-2\" type=\"text/html\"><p>asset 2</p></script>");
            }
        }

        [Fact]
        public void GivenReferenceToBundlesWithSharedPath_WhenRenderEachType_ThenHtmlIsReturnedForEach()
        {
            Action<BundleCollection> config = bundles =>
            {
                bundles.Add<StylesheetBundle>("~/");
                bundles.Add<ScriptBundle>("~/");
            };
            
            using (var app = CreateApplication(config, "overlap"))
            {
                app.OnPostMapRequestHandler();
                var builder = app.GetReferenceBuilder();
                
                builder.Reference("~");
                var scriptHtml = builder.Render<ScriptBundle>(null);
                var stylesheetHtml = builder.Render<StylesheetBundle>(null);

                stylesheetHtml.ShouldContain("<link");
                scriptHtml.ShouldContain("<script");
            }
        }

        CassetteApplication CreateApplication(Action<BundleCollection> configure, string sourceDirectory = "assets", bool isDebuggingEnabled = false)
        {
            var container = new Mock<ICassetteApplicationContainer<ICassetteApplication>>();
            var settings = new CassetteSettings("")
            {
                CacheDirectory = new IsolatedStorageDirectory(storage),
                SourceDirectory = new FileSystemDirectory(Path.GetFullPath(sourceDirectory)),
                UrlGenerator = new UrlGenerator(new VirtualDirectoryPrepender("/"), "_cassette"),
                IsDebuggingEnabled = isDebuggingEnabled
            };
            var bundles = new BundleCollection(settings);
            configure(bundles);
            foreach (var bundle in bundles)
            {
                bundle.Process(settings);
            }
            var bundleContainer = new BundleContainer(bundles);
            var application = new CassetteApplication(
                bundleContainer,
                settings,
                () => httpContext.Object
            );
            container.Setup(c => c.Application).Returns(() => application);
            new RouteInstaller(container.Object, "_cassette").InstallRoutes(routes);
            return application;
        }

        void RemoveExistingCache()
        {
            using (var storage = IsolatedStorageFile.GetMachineStoreForDomain())
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
            Context.SetupGet(c => c.Items)
                   .Returns(new Dictionary<string, object>());

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
