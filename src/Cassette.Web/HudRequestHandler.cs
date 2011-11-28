using System.Collections.Generic;
using System.Linq;
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
        readonly IBundleContainer bundleContainer;
        readonly RequestContext requestContext;
        readonly IUrlGenerator urlGenerator;

        public HudRequestHandler(IBundleContainer bundleContainer, RequestContext requestContext, IUrlGenerator urlGenerator)
        {
            this.bundleContainer = bundleContainer;
            this.requestContext = requestContext;
            this.urlGenerator = urlGenerator;
        }

        public void ProcessRequest(HttpContext _)
        {
            var response = requestContext.HttpContext.Response;
            if (requestContext.HttpContext.Request.Url.Query == "?knockout.js")
            {
                response.ContentType = "application/json";
                response.Write(Properties.Resources.knockout);
                return;
            }

            var htm = Properties.Resources.hud;
            var json = CreateJson();
            htm = htm.Replace("$json$", json)
                     .Replace("$knockout$", Properties.Resources.knockout);

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
                Scripts =
                    from script in scripts
                    select new
                    {
                        script.Path,
                        Url = urlGenerator.CreateBundleUrl(script),
                        Assets = AssetPaths(script)
                    },
                Stylesheets =
                    from stylesheet in stylesheets
                    select new
                    {
                        stylesheet.Path,
                        Url = urlGenerator.CreateBundleUrl(stylesheet),
                        stylesheet.Media,
                        stylesheet.Condition,
                        Assets = AssetPaths(stylesheet)
                    },
                HtmlTemplates =
                    from htmlTemplate in htmlTemplates
                    select new
                    {
                        htmlTemplate.Path,
                        Url = urlGenerator.CreateBundleUrl(htmlTemplate),
                        Assets = AssetPaths(htmlTemplate)
                    }
            };
            var json = new JavaScriptSerializer().Serialize(data);
            return json;
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
            }

            public void Visit(IAsset asset)
            {
                var path = asset.SourceFile.FullPath;
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