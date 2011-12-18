using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Web;
using System.Web.Routing;
using System.Web.Script.Serialization;
using Cassette.HtmlTemplates;
using Cassette.Scripts;
using Cassette.Stylesheets;
using System;
using Cassette.Configuration;

namespace Cassette.Web
{
    class HudRequestHandler : IHttpHandler
    {
        readonly IBundleContainer bundleContainer;
        readonly RequestContext requestContext;
        readonly IUrlGenerator urlGenerator;
        readonly CassetteSettings settings;

        public HudRequestHandler(IBundleContainer bundleContainer, RequestContext requestContext, IUrlGenerator urlGenerator, CassetteSettings settings)
        {
            this.bundleContainer = bundleContainer;
            this.requestContext = requestContext;
            this.urlGenerator = urlGenerator;
            this.settings = settings;
        }

        public void ProcessRequest(HttpContext _)
        {
            var response = requestContext.HttpContext.Response;
            var request = requestContext.HttpContext.Request;

            // Security: Only allow local requests to access the HUD.
            // Perhaps add a config setting for this in the future?
            if (!request.IsLocal)
            {
                response.StatusCode = 404;
                return;
            }

            if (request.Url.Query == "?knockout.js")
            {
                response.ContentType = "application/json";
                response.Write(Properties.Resources.knockout);
                return;
            }

            var htm = Properties.Resources.hud;
            var json = CreateJson();
            htm = htm.Replace("$json$", json);

            response.ContentType = "text/html";
            response.Write(htm);
        }

        string CreateJson()
        {
            var scripts = bundleContainer.Bundles.OfType<ScriptBundle>();
            var stylesheets = bundleContainer.Bundles.OfType<StylesheetBundle>();
            var htmlTemplates = bundleContainer.Bundles.OfType<HtmlTemplateBundle>();

            var data = new
            {
                Scripts = scripts.Select(ScriptData),
                Stylesheets = stylesheets.Select(StylesheetData),
                HtmlTemplates = htmlTemplates.Select(HtmlTemplateData),
                StartupTrace = StartUp.TraceOutput,
                Cassette = new
                {
                    Version = new AssemblyName(typeof(ICassetteApplication).Assembly.FullName).Version.ToString(),
                    CacheDirectory = settings.CacheDirectory.FullPath + " (" + settings.CacheDirectory.GetType().FullName + ")",
                    SourceDirectory = settings.SourceDirectory.FullPath + " (" + settings.SourceDirectory.GetType().FullName + ")",
                    settings.IsHtmlRewritingEnabled,
                    settings.IsDebuggingEnabled
                }
            };
            var json = new JavaScriptSerializer().Serialize(data);
            return json;
        }

        object HtmlTemplateData(HtmlTemplateBundle htmlTemplate)
        {
            return new
            {
                htmlTemplate.Path,
                Url = urlGenerator.CreateBundleUrl(htmlTemplate),
                Assets = AssetPaths(htmlTemplate),
                htmlTemplate.References,
                Size = BundleSize(htmlTemplate)
            };
        }

        object StylesheetData(StylesheetBundle stylesheet)
        {
            return new
            {
                stylesheet.Path,
                Url = urlGenerator.CreateBundleUrl(stylesheet),
                stylesheet.Media,
                stylesheet.Condition,
                Assets = AssetPaths(stylesheet),
                stylesheet.References,
                Size = BundleSize(stylesheet)
            };
        }

        object ScriptData(ScriptBundle script)
        {
            return new
            {
                script.Path,
                Url = urlGenerator.CreateBundleUrl(script),
                Assets = AssetPaths(script),
                script.References,
                Size = BundleSize(script)
            };
        }

        long BundleSize(Bundle script)
        {
            if (script.Assets.Count == 1 && script.Assets[0] is BundleProcessing.ConcatenatedAsset)
            {
                using (var s = script.OpenStream())
                {
                    return s.Length;
                }
            }
            return -1;
        }

        IEnumerable<AssetLink> AssetPaths(Bundle bundle)
        {
            var visitor = new CollectAssetPaths(urlGenerator);
            bundle.Accept(visitor);
            return visitor.AssetLinks;
        }

        public bool IsReusable
        {
            get { return false; }
        }

        class CollectAssetPaths : IBundleVisitor
        {
            readonly IUrlGenerator urlGenerator;
            readonly List<AssetLink> assetLinks = new List<AssetLink>();
            string bundlePath;

            public CollectAssetPaths(IUrlGenerator urlGenerator)
            {
                this.urlGenerator = urlGenerator;
            }

            public List<AssetLink> AssetLinks
            {
                get { return assetLinks; }
            }

            public void Visit(Bundle bundle)
            {
                bundlePath = bundle.Path;
            }

            public void Visit(IAsset asset)
            {
                var path = asset.SourceFile.FullPath;
                if (path.StartsWith(bundlePath, StringComparison.OrdinalIgnoreCase))
                {
                    path = path.Substring(bundlePath.Length + 1);
                }
                var url = urlGenerator.CreateAssetUrl(asset);
                AssetLinks.Add(new AssetLink { Path = path, Url = url });
            }
        }

        class AssetLink
        {
            public string Path { get; set; }
            public string Url { get; set; }
        }
    }
}