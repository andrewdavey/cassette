using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Web;
using System.Web.Routing;
using System.Web.Script.Serialization;
using Cassette.HtmlTemplates;
using Cassette.Scripts;
using Cassette.Stylesheets;

namespace Cassette.Web
{
    class HudRequestHandler : IHttpHandler
    {
        readonly ICassetteApplicationContainer<ICassetteApplication> container;
        readonly RequestContext requestContext;
        IUrlGenerator urlGenerator;

        public HudRequestHandler(ICassetteApplicationContainer<ICassetteApplication> container, RequestContext requestContext)
        {
            this.container = container;
            this.requestContext = requestContext;
        }

        public void ProcessRequest(HttpContext _)
        {
            var response = requestContext.HttpContext.Response;
            var request = requestContext.HttpContext.Request;

            if (!CanAccessHud(request))
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

            if (request.HttpMethod.Equals("post", StringComparison.OrdinalIgnoreCase))
            {
                ProcessPost();
                return;
            }

            var html = Properties.Resources.hud;
            var json = CreateJson();
            html = html.Replace("$json$", json);

            response.ContentType = "text/html";
            response.Write(html);
        }

        void ProcessPost()
        {
            if (requestContext.HttpContext.Request.Form["action"] == "rebuild-cache")
            {
                Application.Settings.CassetteManifestCache.Clear();
                container.RecycleApplication();
            }
        }

        bool CanAccessHud(HttpRequestBase request)
        {
            return request.IsLocal || Application.Settings.AllowRemoteDiagnostics;
        }

        string CreateJson()
        {
            var settings = Application.Settings;
            urlGenerator = settings.UrlGenerator;

            var scripts = Application.Bundles.OfType<ScriptBundle>();
            var stylesheets = Application.Bundles.OfType<StylesheetBundle>();
            var htmlTemplates = Application.Bundles.OfType<HtmlTemplateBundle>();

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

        ICassetteApplication Application
        {
            get { return container.Application; }
        }

        object HtmlTemplateData(HtmlTemplateBundle htmlTemplate)
        {
            return new
            {
                htmlTemplate.Path,
                Url = BundleUrl(htmlTemplate),
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
                Url = BundleUrl(stylesheet),
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
                Url = BundleUrl(script),
                Assets = AssetPaths(script),
                script.References,
                Size = BundleSize(script)
            };
        }

        string BundleUrl(Bundle bundle)
        {
            var external = bundle as IExternalBundle;
            if (external == null)
            {
                return bundle.IsProcessed ? urlGenerator.CreateBundleUrl(bundle) : null;
            }
            else
            {
                return external.Url;
            }
        }

        long BundleSize(Bundle bundle)
        {
            if (bundle.IsProcessed)
            {
                using (var s = bundle.OpenStream())
                {
                    return s.Length;
                }
            }
            return -1;
        }

        IEnumerable<AssetLink> AssetPaths(Bundle bundle)
        {
            var generateUrls = container.Application.Settings.IsDebuggingEnabled;
            var visitor = generateUrls ? new AssetLinkCreator(urlGenerator) : new AssetLinkCreator();
            bundle.Accept(visitor);
            return visitor.AssetLinks;
        }

        public bool IsReusable
        {
            get { return false; }
        }

        class AssetLinkCreator : IBundleVisitor
        {
            readonly IUrlGenerator urlGenerator;
            readonly List<AssetLink> assetLinks = new List<AssetLink>();
            string bundlePath;

            public AssetLinkCreator(IUrlGenerator urlGenerator)
            {
                this.urlGenerator = urlGenerator;
            }

            public AssetLinkCreator()
            {
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
                if (path.Length > bundlePath.Length && path.StartsWith(bundlePath, StringComparison.OrdinalIgnoreCase))
                {
                    path = path.Substring(bundlePath.Length + 1);
                }
                var url = urlGenerator != null ? urlGenerator.CreateAssetUrl(asset) : null;
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