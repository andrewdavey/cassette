using System;
using System.Collections.Specialized;
using System.IO;
using System.IO.IsolatedStorage;
using System.Text.RegularExpressions;
using System.Web;
using System.Web.Routing;
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

        CassetteApplication CreateApplication(Action<ModuleConfiguration> configure)
        {
            var config = new Mock<ICassetteConfiguration>();
            config.Setup(c => c.Configure(It.IsAny<ModuleConfiguration>()))
                  .Callback<ModuleConfiguration>(configure);

            return new CassetteApplication(
                config.Object,
                new FileSystem(Path.GetFullPath(@"..\..\assets")),
                new IsolatedStorageFileSystem(storage),
                isOutputOptmized: true,
                version: Guid.NewGuid().ToString(), // unique version
                urlGenerator: new UrlGenerator("/")
            );
        }

        [Fact]
        public void CanGetScriptModuleA()
        {
            var application = CreateApplication(modules =>
            {
                modules.Add(new PerSubDirectorySource<ScriptModule>("scripts")
                {
                    Exclude = new Regex(@"\.vsdoc\.js$")
                });
            });
            application.InstallRoutes(routes);

            using (var http = new HttpTestHarness(routes))
            {
                http.Get("~/_assets/scripts/scripts/module-a");
                http.ResponseOutputStream.ReadToEnd().ShouldEqual("function asset2(){}function asset1(){}");
            }
        }

        [Fact]
        public void CanGetScriptModuleB()
        {
            var application = CreateApplication(modules =>
            {
                modules.Add(new PerSubDirectorySource<ScriptModule>("scripts")
                {
                    Exclude = new Regex(@"\.vsdoc\.js$")
                });
            });
            application.InstallRoutes(routes);

            using (var http = new HttpTestHarness(routes))
            {
                http.Get("~/_assets/scripts/scripts/module-b");
                http.ResponseOutputStream.ReadToEnd().ShouldEqual("function asset3(){}");
            }
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