using System.Web;
using Cassette.Aspnet;
using Cassette.BundleProcessing;
using Cassette.HtmlTemplates;
using Should;
using Xunit;

namespace Cassette
{
    public class JavaScriptHtmlTemplatePipelineTests
    {
        HttpContextBase httpContext;

        [Fact]
        public void HtmlTemplatesServedAsJavaScript()
        {
            using (var host = new TestableWebHost(".", () => httpContext, true))
            {
                host.ConfigureContainer(c =>
                {
                    c.Register<IBundlePipeline<HtmlTemplateBundle>, JavaScriptHtmlTemplatePipeline>();
                });

                host.AddBundleConfiguration(new BundleConfiguration(
                    bundles => bundles.Add<HtmlTemplateBundle>("~/templates")
                ));

                host.Initialize();
                
                var javaScript = DownloadJavaScript(host, "/cassette.axd/htmltemplate/hash/templates");
                javaScript.ShouldEqual(@"(function(n){var t=function(t){var r=n.createElement(""script"");r.setAttribute(""type"",""text/html""),r.setAttribute(""id"",t),n.body.appendChild(r)};t(""asset-1"",""<p>asset 1<\/p>""),t(""asset-2"",""<p>asset 2<\/p>"")})(document)");
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
