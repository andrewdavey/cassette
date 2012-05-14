using System.Web;
using Cassette.TinyIoC;
using Moq;
using Should;
using Xunit;

namespace Cassette.Aspnet
{
    public class CassetteHttpHandler_Tests
    {
        readonly CassetteHttpHandler handler;
        readonly Mock<HttpRequestBase> request;
        readonly TinyIoCContainer container;

        public CassetteHttpHandler_Tests()
        {
            container = new TinyIoCContainer();
            request = new Mock<HttpRequestBase>();
            handler = new CassetteHttpHandler(container, request.Object);
        }

        [Fact]
        public void ScriptBundleRequestIsProcessedUsingScriptBundleRequestHandler()
        {
            AssertMapping("/script/hash/test", "ScriptBundleRequestHandler", "~/test");
        }

        [Fact]
        public void StylesheetBundleRequestIsProcessedUsingStylesheetBundleRequestHandler()
        {
            AssertMapping("/stylesheet/hash/test", "StylesheetBundleRequestHandler", "~/test");
        }

        [Fact]
        public void HtmlTemplateBundleRequestIsProcessedUsingHtmlTemplateBundleRequestHandler()
        {
            AssertMapping("/htmltemplate/hash/test", "HtmlTemplateBundleRequestHandler", "~/test");
        }

        [Fact]
        public void ScriptBundleRequestWhereTypeIsUpperCaseIsProcessedUsingScriptBundleRequestHandler()
        {
            AssertMapping("/SCRIPT/hash/test", "ScriptBundleRequestHandler", "~/test");
        }

        [Fact]
        public void AssetRequestIsProcessedUsingAssetRequestHandler()
        {
            AssertMapping("/asset/test.js", "AssetRequestHandler", "~/test.js");
        }

        [Fact]
        public void UnexpectedPathInfoCauses404()
        {
            GivenRequestPathInfo("/unexpected");
            Assert404();
        }

        [Fact]
        public void IncompleteScriptBundlePathInfoCauses404()
        {
            GivenRequestPathInfo("/script/bad");
            Assert404();
        }

        [Fact]
        public void RequestWithEmptyPathInfoIsProcessedUsingDiagnosticHandler()
        {
            var diagnosticHandler = new Mock<IDiagnosticRequestHandler>();
            container.Register(diagnosticHandler.Object);

            GivenRequestPathInfo("");
            handler.ProcessRequest();

            diagnosticHandler.Verify(h => h.ProcessRequest());
        }

        void AssertMapping(string pathInfo, string handlerServiceName, string expectedPath)
        {
            var handlerToDelegateTo = new Mock<ICassetteRequestHandler>();
            container.Register(handlerToDelegateTo.Object, handlerServiceName);

            GivenRequestPathInfo(pathInfo);
            handler.ProcessRequest();

            handlerToDelegateTo.Verify(h => h.ProcessRequest(expectedPath));
        }

        void Assert404()
        {
            var exception = Assert.Throws<HttpException>(() => handler.ProcessRequest());
            exception.GetHttpCode().ShouldEqual(404);
        }

        void GivenRequestPathInfo(string pathInfo)
        {
            request.SetupGet(r => r.PathInfo).Returns(pathInfo);
        }
    }
}