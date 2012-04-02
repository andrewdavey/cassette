using System;
using System.Web.Routing;
using Cassette.Configuration;
using Cassette.HtmlTemplates;
using Cassette.Scripts;
using Cassette.Stylesheets;

namespace Cassette.Web
{
    class RouteInstaller : IStartUpTask
    {
        readonly RouteCollection routeCollection;
        readonly BundleCollection bundles;
        readonly CassetteSettings settings;
        readonly IUrlGenerator urlGenerator;
        readonly string routePrefix;

        public RouteInstaller(RouteCollection routeCollection, BundleCollection bundles, CassetteSettings settings, IUrlGenerator urlGenerator)
        {
            this.routeCollection = routeCollection;
            this.bundles = bundles;
            this.settings = settings;
            this.urlGenerator = urlGenerator;
            this.routePrefix = "_cassette";
        }

        public void Run()
        {
            InstallRoutes();
        }

        void InstallRoutes()
        {
            using (routeCollection.GetWriteLock())
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
            var handler = new DelegateRouteHandler(context => new HudRequestHandler(context, bundles, settings, urlGenerator));
            Trace.Source.TraceInformation("Installing diagnostics route handler for \"_cassette\".");
            InsertRouteIntoTable(url, handler);
        }

        void InstallBundleRoute<T>()
            where T : Bundle
        {
            var url = GetBundleRouteUrl<T>();
            var handler = new DelegateRouteHandler(
                requestContext => new BundleRequestHandler<T>(bundles, requestContext)
            );
            Trace.Source.TraceInformation("Installing {0} route handler for \"{1}\".", typeof(T).FullName, url);
            InsertRouteIntoTable(url, handler);
        }

        string GetBundleRouteUrl<T>()
        {
            return String.Format(
                "{0}/{1}/{{*path}}",
                routePrefix,
                ConventionalBundlePathName(typeof(T))
            );
        }

        string ConventionalBundlePathName(Type bundleType)
        {
            // ExternalScriptBundle subclasses ScriptBundle, but we want the name to still be "scripts"
            // So walk up the inheritance chain until we get to something that directly inherits from Bundle.
            while (bundleType != null && bundleType.BaseType != typeof(Bundle))
            {
                bundleType = bundleType.BaseType;
            }
            if (bundleType == null) throw new ArgumentException("Type must be a subclass of Cassette.Bundle.", "bundleType");

            return bundleType.Name.ToLowerInvariant();
        }

        void InstallAssetCompileRoute()
        {
            // Used to return compiled coffeescript, less, etc.
            var url = routePrefix + "/asset/{*path}";
            var handler = new DelegateRouteHandler(
                requestContext => new AssetRequestHandler(
                    requestContext,
                    bundles
                )
            );
            Trace.Source.TraceInformation("Installing asset route handler for \"{0}\".", url);
            InsertRouteIntoTable(url, handler);
        }

        void InstallRawFileRoute()
        {
            var url = routePrefix + "/file/{*path}";
            var handler = new DelegateRouteHandler(
                requestContext => new RawFileRequestHandler(requestContext, bundles, settings)
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
            routeCollection.Insert(0, route);
        }
    }
}