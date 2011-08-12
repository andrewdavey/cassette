using System.Web;
using System.Web.Routing;
using Cassette.UI;

namespace Cassette.Web
{
    public class CassetteApplication : Cassette.CassetteApplicationBase
    {
        public CassetteApplication(IFileSystem sourceFileSystem, IFileSystem cacheFileSystem, UrlGenerator urlGenerator, bool isOutputOptmized, string version)
            : base(sourceFileSystem, cacheFileSystem, isOutputOptmized, version)
        {
            this.urlGenerator = urlGenerator;
        }

        readonly UrlGenerator urlGenerator;
        static readonly string PlaceholderTrackerKey = typeof(IPlaceholderTracker).FullName;

        public override string CreateAbsoluteUrl(string path)
        {
            return VirtualPathUtility.ToAbsolute("~/" + path);
        }

        public override string CreateAssetUrl(Module module, IAsset asset)
        {
            return urlGenerator.CreateAssetUrl(module, asset);
        }

        public override string CreateModuleUrl(Module module)
        {
            return urlGenerator.CreateModuleUrl(module);
        }

        public override IPageAssetManager<T> GetPageAssetManager<T>()
        {
            var items = HttpContext.Current.Items;
            var key = typeof(IPageAssetManager<T>).FullName;
            if (items.Contains(key))
            {
                return (IPageAssetManager<T>)items[key];
            }
            else
            {
                var manager = new PageAssetManager<T>(
                    new ReferenceBuilder<T>(GetModuleContainer<T>(), GetModuleFactory<T>()), 
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
            // Insert Cassette's routes at the start of the table, 
            // to avoid conflicts with the application's own routes.
            InstallModuleRoute<ScriptModule>();
            InstallModuleRoute<StylesheetModule>();
            InstallModuleRoute<HtmlTemplateModule>();

            RouteTable.Routes.Insert(0, new Route("_assets/compile/{*path}", new CompileRouteHandler(this))); //coffee, less, etc
        }

        void InstallModuleRoute<T>()
            where T : Module
        {
            var url = urlGenerator.ModuleUrlPattern<T>();
            var handler = new ModuleRouteHandler<T>(GetModuleContainer<T>());
            RouteTable.Routes.Insert(0, new Route(url, handler));
        }
    }
}
