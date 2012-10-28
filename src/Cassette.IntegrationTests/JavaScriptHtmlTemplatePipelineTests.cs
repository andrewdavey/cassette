using System.Web;
using Cassette.Aspnet;
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
            using (var host = new TestableWebHost(".", () => httpContext))
            {
                host.AddBundleConfiguration(new BundleConfiguration(
                    bundles => bundles.Add<HtmlTemplateBundle>("~/templates", b => b.AsJavaScript())
                ));

                host.Initialize();

                var javaScript = DownloadJavaScript(host, "/cassette.axd/htmltemplate/hash/templates");
                javaScript.ShouldEqual(@"(function(n){var t=function(t,i){var r=n.createElement(""script""),u;r.type=""text/html"",r.id=t,typeof r.textContent!=""undefined""?r.textContent=i:r.innerText=i,u=n.getElementsByTagName(""script"")[0],u.parentNode.insertBefore(r,u)};t(""asset-1"",""<p>asset 1<\/p>""),t(""asset-2"",""<p>asset 2<\/p>"")})(document)");
            }
        }

        string DownloadJavaScript(WebHost host, string url)
        {
            using (var http = new HttpTestHarness(host))
            {
                httpContext = http.Context.Object;

                http.Get(url);
                http.Response.VerifySet(r => r.ContentType = "text/javascript");
                return http.WrittenBody;
            }
        }
    }
}
