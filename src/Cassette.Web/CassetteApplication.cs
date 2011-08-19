using System;
using System.Web;
using System.Web.Routing;
using Cassette.HtmlTemplates;
using Cassette.Scripts;
using Cassette.Stylesheets;
using Cassette.UI;

namespace Cassette.Web
{
    public class CassetteApplication : Cassette.CassetteApplicationBase
    {
        public CassetteApplication(ICassetteConfiguration config, IFileSystem sourceFileSystem, IFileSystem cacheFileSystem, bool isOutputOptmized, string version, UrlGenerator urlGenerator)
            : base(config, sourceFileSystem, cacheFileSystem, urlGenerator, isOutputOptmized, version)
        {
            this.urlGenerator = urlGenerator;
        }

        readonly UrlGenerator urlGenerator;
        static readonly string PlaceholderTrackerKey = typeof(IPlaceholderTracker).FullName;

        public Func<HttpContextBase> GetHttpContext = GetDefaultHttpContext;

        public override IPageAssetManager<T> GetPageAssetManager<T>()
        {
            var items = GetHttpContext().Items;
            var key = typeof(IPageAssetManager<T>).FullName;
            if (items.Contains(key))
            {
                return (IPageAssetManager<T>)items[key];
            }
            else
            {
                var manager = new PageAssetManager<T>(
                    CreateReferenceBuilder<T>(), 
                    this,
                    (IPlaceholderTracker)items[PlaceholderTrackerKey]
                );
                items[key] = manager;
                return manager;
            }
        }

        public void OnBeginRequest(HttpContextBase httpContext)
        {
            var tracker = new PlaceholderTracker();
            httpContext.Items[PlaceholderTrackerKey] = tracker;

            var response = httpContext.Response;
            response.Filter = new PlaceholderReplacingResponseFilter(
                response,
                tracker
            );
        }

        public void InstallRoutes(RouteCollection routes)
        {
            if (IsOutputOptimized)
            {
                InstallModuleRoute<ScriptModule>(routes);
                InstallModuleRoute<StylesheetModule>(routes);
                InstallModuleRoute<HtmlTemplateModule>(routes);

                InstallImageRoute(routes);
            }
            else
            {
                InstallAssetRoute(routes);
            }
        }

        void InstallModuleRoute<T>(RouteCollection routes)
            where T : Module
        {
            // Insert Cassette's routes at the start of the table, 
            // to avoid conflicts with the application's own routes.
            var url = urlGenerator.GetModuleRouteUrl<T>();
            var handler = new ModuleRouteHandler<T>(GetModuleContainer<T>());
            routes.Insert(0, new Route(url, handler));
        }

        void InstallImageRoute(RouteCollection routes)
        {
            routes.Insert(0, new Route(urlGenerator.GetImageRouteUrl(), new ImageRouteHandler()));
        }

        void InstallAssetRoute(RouteCollection routes)
        {
            // Used to return compiled coffeescript, less, etc.
            // Insert Cassette's routes at the start of the table, 
            // to avoid conflicts with the application's own routes.
            var url = urlGenerator.GetAssetRouteUrl();
            var handler = new AssetRouteHandler(this);
            routes.Insert(0, new Route(url, handler));
        }

        static HttpContextBase GetDefaultHttpContext()
        {
            return new HttpContextWrapper(HttpContext.Current);
        }
    }
}
