using System;
using System.Web;
using System.Web.Handlers;
using System.Web.Routing;
using Cassette.HtmlTemplates;
using Cassette.IO;
using Cassette.Scripts;
using Cassette.Stylesheets;
using Cassette.UI;

namespace Cassette.Web
{
    public class CassetteApplication : CassetteApplicationBase
    {
        public CassetteApplication(ICassetteConfiguration config, IDirectory sourceFileSystem, IDirectory cacheFileSystem, bool isOutputOptmized, string version, UrlGenerator urlGenerator, RouteCollection routes, Func<HttpContextBase> getCurrentHttpContext)
            : base(config, sourceFileSystem, cacheFileSystem, urlGenerator, isOutputOptmized, version)
        {
            this.urlGenerator = urlGenerator;
            this.getCurrentHttpContext = getCurrentHttpContext;
            InstallRoutes(routes);
        }

        readonly UrlGenerator urlGenerator;
        readonly Func<HttpContextBase> getCurrentHttpContext;
        static readonly string PlaceholderTrackerKey = typeof(IPlaceholderTracker).FullName;

        public void OnBeginRequest(HttpContextBase httpContext)
        {
            if (httpContext.CurrentHandler is AssemblyResourceLoader)
            {
                // The AssemblyResourceLoader handler (for WebResource.axd requests) prevents further writes via some internal puke code.
                // This prevents response filters from working. The result is an empty response body!
                // So don't bother installing a filter for these requests. We don't need to rewrite them anyway.
                return;
            }

            var tracker = new PlaceholderTracker();
            httpContext.Items[PlaceholderTrackerKey] = tracker;

            var response = httpContext.Response;
            response.Filter = new PlaceholderReplacingResponseFilter(
                response,
                tracker
            );
        }

        public override IPageAssetManager GetPageAssetManager<T>()
        {
            var items = getCurrentHttpContext().Items;
            var key = "PageAssetManager:" + typeof(T).FullName;
            if (items.Contains(key))
            {
                return (IPageAssetManager)items[key];
            }
            else
            {
                var manager = new PageAssetManager<T>(
                    CreateReferenceBuilder<T>(),
                    (IPlaceholderTracker)items[PlaceholderTrackerKey]
                );
                items[key] = manager;
                return manager;
            }
        }

        void InstallRoutes(RouteCollection routes)
        {
            InstallModuleRoute<ScriptModule>(routes);
            InstallModuleRoute<StylesheetModule>(routes);
            InstallModuleRoute<HtmlTemplateModule>(routes);

            InstallImageRoute(routes);

            InstallAssetRoute(routes);
        }

        void InstallModuleRoute<T>(RouteCollection routes)
            where T : Module
        {
            // Insert Cassette's routes at the start of the table, 
            // to avoid conflicts with the application's own routes.
            var url = urlGenerator.GetModuleRouteUrl<T>();
            var handler = new ModuleRouteHandler<T>(GetModuleContainer<T>());
            routes.Insert(0, new CassetteRoute(url, handler));
        }

        void InstallImageRoute(RouteCollection routes)
        {
            routes.Insert(0, new CassetteRoute(urlGenerator.GetImageRouteUrl(), new ImageRouteHandler()));
        }

        void InstallAssetRoute(RouteCollection routes)
        {
            // Used to return compiled coffeescript, less, etc.
            // Insert Cassette's routes at the start of the table, 
            // to avoid conflicts with the application's own routes.
            var url = urlGenerator.GetAssetRouteUrl();
            var handler = new AssetRouteHandler(this);
            routes.Insert(0, new CassetteRoute(url, handler));
        }
    }
}
