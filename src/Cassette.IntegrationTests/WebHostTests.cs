using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Web;
using System.Xml.Linq;
using Cassette.Aspnet;
using Cassette.HtmlTemplates;
using Cassette.Scripts;
using Cassette.Stylesheets;
using Cassette.Views;
using Should;
using Xunit;

namespace Cassette
{
    public class WebHostTests
    {
        HttpContextBase httpContext;

        [Fact]
        public void PageThatReferencesScriptsBundleAGetsScriptsBundleBAndScriptsBundleA()
        {
            using (var host = new TestableWebHost("assets", () => httpContext))
            {
                host.AddBundleConfiguration(new BundleConfiguration(bundles => 
                    bundles.AddPerSubDirectory<ScriptBundle>("scripts")
                ));
                host.Initialize();

                var scriptUrls = GetPageHtmlResourceUrls(host, "scripts/bundle-a");

                scriptUrls[0].ShouldMatch(new Regex(@"^/cassette\.axd/script/[^/]+/scripts/bundle-b"));
                Download(host, scriptUrls[0]).ShouldEqual(@"function asset3(){}");

                scriptUrls[1].ShouldMatch(new Regex(@"^/cassette\.axd/script/[^/]+/scripts/bundle-a"));
                Download(host, scriptUrls[1]).ShouldEqual(@"function asset2(){}function asset1(){}");
            }
        }

        [Fact] 
        public void PageThatReferencesStylesAndTemplatesDoNotGetDuplicateResources()
        {
            using (var host = new TestableWebHost("assets", () => httpContext))
            {
                host.AddBundleConfiguration(new BundleConfiguration(bundles =>
                {
                    bundles.AddPerSubDirectory<ScriptBundle>("scripts");
                    bundles.AddPerIndividualFile<HtmlTemplateBundle>("templates");
                }));
                host.Initialize();

                var scriptUrls = GetPageHtmlResourceUrls(host, "scripts/bundle-a", "templates/asset-1.htm");
                Assert.Equal(scriptUrls.Distinct(), scriptUrls);
            }
        }

        [Fact]
        public void GivenDebugMode_PageThatReferencesScriptsBundleAGetsAssetsForScriptsBundleBAndScriptsBundleA()
        {
            using (var host = new TestableWebHost("assets", () => httpContext, isAspNetDebuggingEnabled: true))
            {
                host.AddBundleConfiguration(new BundleConfiguration(bundles =>
                    bundles.AddPerSubDirectory<ScriptBundle>("scripts")
                ));
                host.Initialize();

                var scriptUrls = GetPageHtmlResourceUrls(host, "scripts/bundle-a");

                scriptUrls[0].ShouldStartWith("/cassette.axd/asset/scripts/bundle-b/asset-3.js?");
                Download(host, scriptUrls[0]).ShouldEqual(File.ReadAllText("assets\\scripts\\bundle-b\\asset-3.js"));

                scriptUrls[1].ShouldStartWith("/cassette.axd/asset/scripts/bundle-a/asset-2.js?");
                Download(host, scriptUrls[1]).ShouldEqual(File.ReadAllText("assets\\scripts\\bundle-a\\asset-2.js"));

                scriptUrls[2].ShouldStartWith("/cassette.axd/asset/scripts/bundle-a/asset-1.js?");
                Download(host, scriptUrls[2]).ShouldEqual(File.ReadAllText("assets\\scripts\\bundle-a\\asset-1.js"));
            }
        }

        [Fact]
        public void PageThatReferencesScriptsBundleCGetsScriptsBundleBAndScriptsBundleC()
        {
            using (var host = new TestableWebHost("assets", () => httpContext))
            {
                host.AddBundleConfiguration(new BundleConfiguration(bundles =>
                    bundles.AddPerSubDirectory<ScriptBundle>("scripts")
                ));
                host.Initialize();

                var scriptUrls = GetPageHtmlResourceUrls(host, "scripts/bundle-c");

                scriptUrls[0].ShouldMatch(new Regex(@"^/cassette\.axd/script/[^/]+/scripts/bundle-c"));
                Download(host, scriptUrls[0]).ShouldEqual(@"(function(){var n;n=1,log(n)}).call(this)");
            }
        }

#if !NET35
        // The styles bundles contain a SASS file - which Cassette for .NET 3.5 doesn't support
        [Fact]
        public void PageThatReferencesStylesBundleAGetsStylesBundleA()
        {
            using (var host = new TestableWebHost("assets", () => httpContext))
            {
                host.AddBundleConfiguration(new BundleConfiguration(bundles =>
                    bundles.AddPerSubDirectory<StylesheetBundle>("styles")
                ));
                host.Initialize();

                var urls = GetPageHtmlResourceUrls(host, "styles/bundle-a");

                Download(host, urls[0]).ShouldEqual("a{color:red}p{border:1px solid red}body{color:#abc}");
            }
        }
#endif

        [Fact]
        public void GivenDebugMode_BundleAUrlShouldServeConcatenatedScripts()
        {
            using (var host = new TestableWebHost("assets", () => httpContext, isAspNetDebuggingEnabled: true))
            {
                host.AddBundleConfiguration(new BundleConfiguration(bundles =>
                    bundles.AddPerSubDirectory<ScriptBundle>("scripts")
                ));
                host.Initialize();

                string bundleUrl;
                using (var http = new HttpTestHarness(host))
                {
                    httpContext = http.Context.Object;
                    Bundles.Reference("scripts/bundle-a");
                    bundleUrl = Bundles.Url("scripts/bundle-a");
                }

                var separator = System.Environment.NewLine + ";" + System.Environment.NewLine;

                var expectedContent = File.ReadAllText("assets\\scripts\\bundle-a\\asset-2.js") + separator +
                                      File.ReadAllText("assets\\scripts\\bundle-a\\asset-1.js");

                Download(host, bundleUrl).ShouldEqual(expectedContent);
            }
        }

        string[] GetPageHtmlResourceUrls(WebHost host, params string[] references)
        {
            using (var http = new HttpTestHarness(host))
            {
                httpContext = http.Context.Object;

                foreach (var reference in references)
                {
                    Bundles.Reference(reference);
                }

                var htmlString = 
                    "<html>" +
                    Bundles.RenderScripts().ToHtmlString() +
                    Bundles.RenderStylesheets().ToHtmlString() +
                    Bundles.RenderHtmlTemplates().ToHtmlString() +
                    "</html>";

                var html = XElement.Parse(htmlString);
                var scripts = html.Elements("script").Where((s => s.Attribute("src") != null)).Select(s => s.Attribute("src").Value);
                var links = html.Elements("link").Select(s => s.Attribute("href").Value);
                return links.Concat(scripts).ToArray();
            }
        }

        string Download(WebHost host, string url)
        {
            using (var http = new HttpTestHarness(host))
            {
                httpContext = http.Context.Object;

                http.Get(url);
                return http.ResponseOutputStream.ReadToEnd();
            }
        }
    }
}