using System;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Web;
using System.Web.Routing;
using System.Xml.Linq;
using Cassette.Configuration;
using Cassette.IntegrationTests;
using Cassette.Scripts;
using Cassette.Views;
using Should;
using Xunit;
#if !NET35
using Cassette.Stylesheets;
#endif

namespace Cassette
{
    public class WebHostTests
    {
        HttpContextBase httpContext;
        readonly RouteCollection routes = new RouteCollection();

        [Fact]
        public void PageThatReferencesScriptsBundleAGetsScriptsBundleBAndScriptsBundleA()
        {
            using (var host = new TestableWebHost("assets", routes, () => httpContext))
            {
                host.AddBundleDefinition(new BundleDefinition(bundles => 
                    bundles.AddPerSubDirectory<ScriptBundle>("scripts")
                ));
                host.Initialize();

                var scriptUrls = GetPageHtmlResourceUrls("scripts/bundle-a");

                scriptUrls[0].ShouldMatch(new Regex("^/_cassette/scriptbundle/scripts/bundle-b_"));
                Download(scriptUrls[0]).ShouldEqual(@"function asset3(){}");

                scriptUrls[1].ShouldMatch(new Regex("^/_cassette/scriptbundle/scripts/bundle-a_"));
                Download(scriptUrls[1]).ShouldEqual(@"function asset2(){}function asset1(){}");
            }
        }

        [Fact]
        public void GivenDebugMode_PageThatReferencesScriptsBundleAGetsAssetsForScriptsBundleBAndScriptsBundleA()
        {
            using (var host = new TestableWebHost("assets", routes, () => httpContext, isAspNetDebuggingEnabled: true))
            {
                host.AddBundleDefinition(new BundleDefinition(bundles =>
                    bundles.AddPerSubDirectory<ScriptBundle>("scripts")
                ));
                host.Initialize();

                var scriptUrls = GetPageHtmlResourceUrls("scripts/bundle-a");

                scriptUrls[0].ShouldMatch(new Regex("^/_cassette/asset/scripts/bundle-b/asset-3.js"));
                Download(scriptUrls[0]).ShouldEqual(File.ReadAllText("assets\\scripts\\bundle-b\\asset-3.js"));

                scriptUrls[1].ShouldMatch(new Regex("^/_cassette/asset/scripts/bundle-a/asset-2.js"));
                Download(scriptUrls[1]).ShouldEqual(File.ReadAllText("assets\\scripts\\bundle-a\\asset-2.js"));

                scriptUrls[2].ShouldMatch(new Regex("^/_cassette/asset/scripts/bundle-a/asset-1.js"));
                Download(scriptUrls[2]).ShouldEqual(File.ReadAllText("assets\\scripts\\bundle-a\\asset-1.js"));
            }
        }

        [Fact]
        public void PageThatReferencesScriptsBundleCGetsScriptsBundleBAndScriptsBundleC()
        {
            using (var host = new TestableWebHost("assets", routes, () => httpContext))
            {
                host.AddBundleDefinition(new BundleDefinition(bundles =>
                    bundles.AddPerSubDirectory<ScriptBundle>("scripts")
                ));
                host.Initialize();

                var scriptUrls = GetPageHtmlResourceUrls("scripts/bundle-c");

                scriptUrls[0].ShouldMatch(new Regex("^/_cassette/scriptbundle/scripts/bundle-c_"));
                Download(scriptUrls[0]).ShouldEqual(@"(function(){var n;n=1}).call(this)");
            }
        }

#if !NET35
        // The styles bundles contain a SASS file - which Cassette for .NET 3.5 doesn't support
        [Fact]
        public void PageThatReferencesStylesBundleAGetsStylesBundleA()
        {
            using (var host = new TestableWebHost("assets", routes, () => httpContext))
            {
                host.AddBundleDefinition(new BundleDefinition(bundles =>
                    bundles.AddPerSubDirectory<StylesheetBundle>("styles")
                ));
                host.Initialize();

                var urls = GetPageHtmlResourceUrls("styles/bundle-a");

                Download(urls[0]).ShouldEqual("a{color:red}p{border:1px solid red}body{color:#abc}");
            }
        }
#endif

        string[] GetPageHtmlResourceUrls(params string[] references)
        {
            using (var http = new HttpTestHarness(routes))
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
                    "</html>";

                var html = XElement.Parse(htmlString);
                var scripts = html.Elements("script").Select(s => s.Attribute("src").Value);
                var links = html.Elements("link").Select(s => s.Attribute("href").Value);
                return links.Concat(scripts).ToArray();
            }
        }

        string Download(string url)
        {
            using (var http = new HttpTestHarness(routes))
            {
                httpContext = http.Context.Object;

                http.Get(url);
                return http.ResponseOutputStream.ReadToEnd();
            }
        }

        class BundleDefinition : IBundleDefinition
        {
            readonly Action<BundleCollection> addBundles;

            public BundleDefinition(Action<BundleCollection> addBundles)
            {
                this.addBundles = addBundles;
            }

            public void AddBundles(BundleCollection bundles)
            {
                addBundles(bundles);
            }
        }
    }
}