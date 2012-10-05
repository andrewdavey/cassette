using System.Web;
using Cassette.Aspnet;
using Cassette.HtmlTemplates;
using Cassette.Interop;
using Should;
using Xunit;

namespace Cassette
{
    public class JavaScriptHtmlTemplatePipelineTests
    {
        HttpContextBase httpContext;

        [Fact]
        public void HtmlTemplatesServedAsDomAppendingJavaScript()
        {
            using (var host = new TestableWebHost(".", () => httpContext))
            {
                host.AddBundleConfiguration(new BundleConfiguration(
                    bundles => bundles.Add<HtmlTemplateBundle>("~/templates")
                ));
                host.ConfigureContainer(new BasicHtmlServicesConfig().Configure);
                host.Initialize();

                var javaScript = DownloadJavaScript(host, "/cassette.axd/htmltemplate/hash/templates");
                javaScript.ShouldEqual(@"(function(n){var t=function(t,i){var r=n.createElement(""script""),u;r.type=""text/html"",r.id=t,typeof r.textContent!=""undefined""?r.textContent=i:r.innerText=i,u=n.getElementsByTagName(""script"")[0],u.parentNode.insertBefore(r,u)};t(""asset-1"",""<p>asset 1<\/p>""),t(""asset-2"",""<p>asset 2<\/p>"")})(document)");
            }
        }

        class BasicHtmlServicesConfig : ServicesConfiguration
        {
            public BasicHtmlServicesConfig()
            {
                ConvertHtmlTemplatesIntoJavaScript();
            }
        }

        [Fact]
        public void HtmlTemplatesServedAsDefaultGlobalVarJavaScript()
        {
            using (var host = new TestableWebHost(".", () => httpContext))
            {
                host.AddBundleConfiguration(new BundleConfiguration(
                    bundles => bundles.Add<HtmlTemplateBundle>("~/templates")
                ));
                host.ConfigureContainer(new GlobalVarHtmlServicesConfig().Configure);
                host.Initialize();

                var javaScript = DownloadJavaScript(host, "/cassette.axd/htmltemplate/hash/templates");
                javaScript.ShouldEqual(@"(function(n){n[""asset-1""]=""<p>asset 1<\/p>"",n[""asset-2""]=""<p>asset 2<\/p>""})(window.JST||(window.JST={}))");
            }
        }

        [Fact]
        public void HtmlTemplatesServedAsCustomGlobalVarJavaScript()
        {
            using (var host = new TestableWebHost(".", () => httpContext))
            {
                host.AddBundleConfiguration(new BundleConfiguration(
                    bundles => bundles.Add<HtmlTemplateBundle>("~/templates")
                ));
                host.ConfigureContainer(new GlobalVarHtmlServicesConfig("templates").Configure);
                host.Initialize();

                var javaScript = DownloadJavaScript(host, "/cassette.axd/htmltemplate/hash/templates");
                javaScript.ShouldEqual(@"(function(n){n[""asset-1""]=""<p>asset 1<\/p>"",n[""asset-2""]=""<p>asset 2<\/p>""})(window.templates||(window.templates={}))");

                using (var engine = new IEJavaScriptEngine())
                {
                    engine.Initialize();
                    engine.LoadLibrary("var window={};");
                    engine.LoadLibrary(javaScript);
                    engine.LoadLibrary("function getTemplate(id) { return window.templates[id]; }");
                    var asset1 = engine.CallFunction<string>("getTemplate", "asset-1");
                    asset1.ShouldEqual("<p>asset 1</p>");
                }
            }
        }

        [Fact]
        public void HtmlTemplatesAsDomAppendingJavaScriptReturnsValidJavaScript()
        {
            using (var host = new TestableWebHost(".", () => httpContext))
            {
                host.AddBundleConfiguration(new BundleConfiguration(
                    bundles => bundles.Add<HtmlTemplateBundle>("~/templates")
                ));
                host.ConfigureContainer(new BasicHtmlServicesConfig().Configure);
                host.Initialize();

                var javaScript = DownloadJavaScript(host, "/cassette.axd/htmltemplate/hash/templates");

                using (var engine = new IEJavaScriptEngine())
                {

                    engine.Initialize();
                    // Mock enough of document for the template definition code to use.
                    engine.LoadLibrary(@"var document= {
    getElementsByTagName: function() { return [ { parentNode: { insertBefore: function() {} } } ]; },
    createElement: function() { return {}; }
};");
                    // Just test that the template definition code runs without throwing
                    // i.e. it's valid JS.
                    Assert.DoesNotThrow(() => engine.LoadLibrary(javaScript));
                }
            }
        }

        [Fact]
        public void HtmlTemplatesServedAsCustomGlobalVarJavaScriptReturnsValidJavaScript()
        {
            using (var host = new TestableWebHost(".", () => httpContext))
            {
                host.AddBundleConfiguration(new BundleConfiguration(
                    bundles => bundles.Add<HtmlTemplateBundle>("~/templates")
                ));
                host.ConfigureContainer(new GlobalVarHtmlServicesConfig("templates").Configure);
                host.Initialize();

                var javaScript = DownloadJavaScript(host, "/cassette.axd/htmltemplate/hash/templates");
                
                using (var engine = new IEJavaScriptEngine())
                {
                    engine.Initialize();
                    engine.LoadLibrary("var window={};");
                    engine.LoadLibrary(javaScript);
                    engine.LoadLibrary("function getTemplate(id) { return window.templates[id]; }");
                    var asset1 = engine.CallFunction<string>("getTemplate", "asset-1");
                    asset1.ShouldEqual("<p>asset 1</p>");
                }
            }
        }

        class GlobalVarHtmlServicesConfig : ServicesConfiguration
        {
            public GlobalVarHtmlServicesConfig()
            {
                ConvertHtmlTemplatesIntoJavaScript()
                    .StoreInGlobalVariable();
            }

            public GlobalVarHtmlServicesConfig(string globalVarName)
            {
                ConvertHtmlTemplatesIntoJavaScript()
                    .StoreInGlobalVariable(globalVarName);
            }
        }

        string DownloadJavaScript(WebHost host, string url)
        {
            using (var http = new HttpTestHarness(host))
            {
                httpContext = http.Context.Object;

                http.Get(url);
                http.Response.VerifySet(r => r.ContentType = "text/javascript");
                return http.ResponseOutputStream.ReadToEnd();
            }
        }
    }
}
