using System.Web.Routing;
using Cassette.HtmlTemplates;
using Cassette.Scripts;
using Cassette.Stylesheets;

namespace Cassette.Web
{
    class RouteInstaller
    {
        readonly ICassetteApplicationContainer<ICassetteApplication> container;
        readonly string routePrefix;
        RouteCollection routes;

        public RouteInstaller(ICassetteApplicationContainer<ICassetteApplication> container, string routePrefix)
        {
            this.container = container;
            this.routePrefix = routePrefix;
        }

        public void InstallRoutes(RouteCollection routeCollection)
        {
            routes = routeCollection;
            using (routes.GetWriteLock())
            {
                InstallHudRoute();
                InstallBundleRoute<ScriptBundle>();
                InstallBundleRoute<StylesheetBundle>();
                InstallBundleRoute<HtmlTemplateBundle>();
                InstallAssetCompileRoute();
                InstallRawFileRoute();
            }
        }

        void InstallHudRoute()
        {
            var url = routePrefix;
            var handler = new DelegateRouteHandler(context => new HudRequestHandler(container, context));
            Trace.Source.TraceInformation("Installing diagnostics route handler for \"_cassette\".");
            InsertRouteIntoTable(url, handler);
        }

        public string CreateAssetUrl(IAsset asset)
        {
            return urlModifier.Modify(string.Format(
                "{0}/asset/{1}?{2}",
                RoutePrefix,
                asset.SourceFile.FullPath.Substring(2),
                asset.Hash.ToHexString()
            ));
        }

        public string CreateRawFileUrl(string filename, string hash)
        {
            if (filename.StartsWith("~") == false)
            {
                throw new ArgumentException("Image filename must be application relative (starting with '~').");
            }

            filename = filename.Substring(2); // Remove the "~/"
            var dotIndex = filename.LastIndexOf('.');
            var name = filename.Substring(0, dotIndex);
            var extension = filename.Substring(dotIndex + 1);
            
            return urlModifier.Modify(string.Format("{0}/file/{1}_{2}.{3}",
                RoutePrefix,
                ConvertToForwardSlashes(name),
                hash,
                extension
            ));
        }

        static void RemoveExistingCassetteRoutes(RouteCollection routes)
        {
            for (int i = routes.Count - 1; i >= 0; i--)
            {
                if (routes[i] is CassetteRoute)
                {
                    routes.RemoveAt(i);
                }
            }
        }

        void InstallBundleRoute<T>(RouteCollection routes)
            where T : Bundle
        {
            var url = GetBundleRouteUrl<T>();
            var handler = new DelegateRouteHandler(
                requestContext => new BundleRequestHandler<T>(container.Application, requestContext)
            );
            Trace.Source.TraceInformation("Installing {0} route handler for \"{1}\".", typeof(T).FullName, url);
            InsertRouteIntoTable(url, handler);
        }

        string GetBundleRouteUrl<T>()
        {
            return string.Format(
                "{0}/{1}/{{*path}}",
                routePrefix,
                RoutingHelpers.ConventionalBundlePathName(typeof(T))
            );
        }

        void InstallAssetCompileRoute()
        {
            // Used to return compiled coffeescript, less, etc.
            var url = routePrefix + "/asset/{*path}";
            var handler = new DelegateRouteHandler(
                requestContext => new AssetRequestHandler(
                    requestContext,
                    container.Application.Bundles
                )
            );
            Trace.Source.TraceInformation("Installing asset route handler for \"{0}\".", url);
            InsertRouteIntoTable(url, handler);
        }

        void InstallRawFileRoute()
        {
            var url = routePrefix + "/file/{*path}";
            var handler = new DelegateRouteHandler(
                requestContext => new RawFileRequestHandler(requestContext)
            );
            Trace.Source.TraceInformation("Installing raw file route handler for \"{0}\".", url);
            InsertRouteIntoTable(url, handler);
        }

        void InsertRouteIntoTable(string url, IRouteHandler routeHandler)
        {
            InsertRouteIntoTable(new CassetteRoute(url, routeHandler));
        }

        void InsertRouteIntoTable(RouteBase route)
        {
            // Insert Cassette's routes at the start of the table, 
            // to avoid conflicts with the application's own routes.
            routes.Insert(0, route);
        }
    }
}