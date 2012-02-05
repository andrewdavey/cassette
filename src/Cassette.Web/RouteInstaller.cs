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

        void InstallBundleRoute<T>()
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
                UrlGenerator.ConventionalBundlePathName(typeof(T))
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